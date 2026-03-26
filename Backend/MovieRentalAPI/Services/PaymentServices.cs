using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Helpers;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Models.Enums;
using MovieRentalModels;
using Microsoft.EntityFrameworkCore;

namespace MovieRentalAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<int, Payment> _paymentRepository;
        private readonly IRepository<int, Rental> _rentalRepository;
        private readonly IRepository<int, RentalItem> _rentalItemRepository;
        private readonly MovieRentalContext _context;
        private readonly NotificationService _notifications;

        public PaymentService(
            IRepository<int, Payment> paymentRepository,
            IRepository<int, Rental> rentalRepository,
            IRepository<int, RentalItem> rentalItemRepository,
            MovieRentalContext context,
            NotificationService notifications)
        {
            _paymentRepository = paymentRepository;
            _rentalRepository = rentalRepository;
            _rentalItemRepository = rentalItemRepository;
            _context = context;
            _notifications = notifications;
        }

        public async Task<PaymentResponseDto> MakePayment(MakePaymentRequestDto request)
        {
            var rental = await _rentalRepository.Get(request.RentalId);

            if (rental == null)
                throw new NotFoundException("Rental not found");

            if (rental.Status != RentalStatus.PaymentPending)
                throw new ConflictException("Payment already processed");

            var paymentId = GeneratePaymentId();

            PaymentStatus paymentStatus;
            RentalStatus rentalStatus;

            if (request.IsSuccess)
            {
                paymentStatus = PaymentStatus.Success;
                rentalStatus = RentalStatus.Available;

                // ✅ ACTIVATE ALL RENTAL ITEMS
                var rentalItemsList = await _rentalItemRepository.GetAll();

                var rentalItems = rentalItemsList?
                    .Where(i => i.RentalId == rental.Id)
                    .ToList();

                if (rentalItems != null)
                {
                    foreach (var item in rentalItems)
                    {
                        item.IsActive = true;
                        await _rentalItemRepository.Update(item.Id, item);
                    }
                }
            }
            else
            {
                paymentStatus = PaymentStatus.Failed;
                rentalStatus = RentalStatus.PaymentDeclined;
            }

            var payment = new Payment
            {
                RentalId = rental.Id,
                PaymentId = paymentId,
                UserId = rental.UserId,
                Amount = rental.TotalAmount,
                PaymentMethod = request.Method,
                Status = paymentStatus,
                PaymentDate = IstDateTime.Now
            };

            var added = await _paymentRepository.Add(payment);

            rental.Status = rentalStatus;
            rental.PaymentId = paymentId;
            await _rentalRepository.Update(rental.Id, rental);

            // Notify user
            if (request.IsSuccess)
                await _notifications.Push(rental.UserId, "payment",
                    "Payment Successful",
                    $"Your payment of ₹{rental.TotalAmount:F2} was successful. Enjoy your movies!", rental.Id);
            else
                await _notifications.Push(rental.UserId, "payment",
                    "Payment Failed",
                    "Your payment could not be processed. Please try again.", rental.Id);

            return new PaymentResponseDto
            {
                Id = added.Id,
                RentalId = added.RentalId,
                PaymentId = added.PaymentId,
                Amount = added.Amount,
                Method = added.PaymentMethod,
                Status = added.Status,
                PaymentDate = added.PaymentDate,
                UserId = added.UserId
            };
        }

        public async Task<PaymentResponseDto> ProcessRefund(int rentalItemId)
        {
            var item = await _rentalItemRepository.Get(rentalItemId);
            if (item == null)
                throw new NotFoundException("Rental item not found");

            var rental = await _rentalRepository.Get(item.RentalId);
            if (rental == null)
                throw new NotFoundException("Rental not found");

            var payments = await _paymentRepository.GetAllIncluding(p => p.User);
            var payment = payments?.FirstOrDefault(p => p.RentalId == rental.Id);
            if (payment == null)
                throw new NotFoundException("Payment not found for this rental");

            if (payment.Status != PaymentStatus.Success)
                throw new ConflictException("Payment is not eligible for refund");

            var now = IstDateTime.Now;
            var hoursSincePurchase = (now - payment.PaymentDate).TotalHours;

            double refundPct = hoursSincePurchase <= 2 ? 0.75
                             : hoursSincePurchase <= 4 ? 0.50
                             : 0.0;

            // Use per-item amount: pricePerDay × rental days
            var rentalDays = Math.Max(1, (int)Math.Round((item.EndDate - item.StartDate).TotalDays));
            double itemAmount = item.PricePerDay * rentalDays;
            double refundAmount = itemAmount * refundPct;

            // Deactivate the rental item
            item.IsActive = false;
            await _rentalItemRepository.Update(item.Id, item);

            // Write per-item refund record
            var itemRefund = new RentalItemRefund
            {
                RentalItemId = item.Id,
                RentalId = rental.Id,
                UserId = rental.UserId,
                RefundAmount = refundAmount,
                RefundedAt = now
            };
            _context.RentalItemRefunds.Add(itemRefund);
            await _context.SaveChangesAsync();

            // Update payment status (shared record — just mark as Refunded)
            payment.Status = PaymentStatus.Refunded;
            payment.RefundAmount = refundAmount;
            payment.RefundedAt = now;
            await _paymentRepository.Update(payment.Id, payment);

            // Notify user
            var movie = await _context.Set<Movie>().FindAsync(item.MovieId);
            if (refundAmount > 0)
                await _notifications.Push(rental.UserId, "refund",
                    "Refund Processed",
                    $"₹{refundAmount:F2} has been refunded for \"{movie?.Title ?? "your movie"}\". It will reflect in 3-5 business days.",
                    item.Id);
            else
                await _notifications.Push(rental.UserId, "refund",
                    "Rental Returned",
                    $"Your rental of \"{movie?.Title ?? "the movie"}\" has been returned. No refund is applicable.",
                    item.Id);

            return MapToResponse(payment);
        }

        public async Task<RentalItemRefundDto?> GetItemRefund(int rentalItemId)
        {
            var refund = await _context.RentalItemRefunds
                .Where(r => r.RentalItemId == rentalItemId)
                .OrderByDescending(r => r.RefundedAt)
                .FirstOrDefaultAsync();

            if (refund == null) return null;

            return new RentalItemRefundDto
            {
                RentalItemId = refund.RentalItemId,
                RefundAmount = refund.RefundAmount,
                RefundedAt = refund.RefundedAt
            };
        }

        public async Task<PaymentResponseDto> GetPaymentByRental(int rentalId)
        {
            var rental = await _rentalRepository.Get(rentalId);

            if (rental == null)
                throw new NotFoundException("Rental not found");

            var payments = await _paymentRepository.GetAllIncluding(p => p.User);

            var payment = payments?
                .FirstOrDefault(p => p.RentalId == rentalId);

            if (payment == null)
                throw new NotFoundException("Payment not found for this rental");

            return MapToResponse(payment);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentByUser(int userId)
        {
            var payments = await _paymentRepository.GetAllIncluding(p => p.User);

            var userPayments = payments?
                .Where(p => p.UserId == userId)
                .ToList();

            if (userPayments == null || !userPayments.Any())
                throw new NotFoundException("No payments found for this user");

            return userPayments.Select(MapToResponse);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPayments()
        {
            var payments = await _paymentRepository.GetAllIncluding(p => p.User);

            if (payments == null || !payments.Any())
                throw new NotFoundException("No payments found");

            return payments.Select(MapToResponse);
        }

        private PaymentResponseDto MapToResponse(Payment p)
        {
            return new PaymentResponseDto
            {
                Id = p.Id,
                RentalId = p.RentalId,
                Amount = p.Amount,
                Method = p.PaymentMethod,
                Status = p.Status,
                PaymentDate = p.PaymentDate,
                UserId = p.UserId,
                UserName = p.User != null ? p.User.Name : "N/A",
                PaymentId = p.PaymentId,
                RefundAmount = p.RefundAmount,
                RefundedAt = p.RefundedAt
            };
        }
        private string GeneratePaymentId()
        {
            return "PAY_" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
        }
    }
}