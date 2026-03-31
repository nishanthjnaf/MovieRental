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
        private readonly IActivityLogService _activityLog;

        public PaymentService(
            IRepository<int, Payment> paymentRepository,
            IRepository<int, Rental> rentalRepository,
            IRepository<int, RentalItem> rentalItemRepository,
            MovieRentalContext context,
            NotificationService notifications,
            IActivityLogService activityLog)
        {
            _paymentRepository = paymentRepository;
            _rentalRepository = rentalRepository;
            _rentalItemRepository = rentalItemRepository;
            _context = context;
            _notifications = notifications;
            _activityLog = activityLog;
        }

        // ── PURCHASE ────────────────────────────────────────────────────────────
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

                // Activate all rental items
                var allItems = await _rentalItemRepository.GetAll();
                var rentalItems = allItems?.Where(i => i.RentalId == rental.Id).ToList();
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

            // Create a Purchase payment record
            var payment = new Payment
            {
                RentalId = rental.Id,
                PaymentId = paymentId,
                UserId = rental.UserId,
                Amount = rental.TotalAmount,
                PaymentMethod = request.Method,
                PaymentType = PaymentType.Purchase,
                Status = paymentStatus,
                PaymentDate = IstDateTime.Now
            };

            var added = await _paymentRepository.Add(payment);

            rental.Status = rentalStatus;
            rental.PaymentId = paymentId;
            await _rentalRepository.Update(rental.Id, rental);

            if (request.IsSuccess)
                await _notifications.Push(rental.UserId, "payment",
                    "Payment Successful",
                    $"Your payment of ₹{rental.TotalAmount:F2} was successful. Enjoy your movies!", rental.Id);
            else
                await _notifications.Push(rental.UserId, "payment",
                    "Payment Failed",
                    "Your payment could not be processed. Please try again.", rental.Id);

            var logStatus = request.IsSuccess ? "Success" : "Failure";
            await _activityLog.Log(rental.UserId, added.User?.Name ?? added.User?.Username ?? "Customer", "Customer",
                "Payment", "MakePayment",
                $"Payment {added.PaymentId} for Rental #{rental.Id}. Amount: ₹{rental.TotalAmount:F2}. Status: {paymentStatus}.",
                logStatus);

            return MapToResponse(added);
        }

        // ── REFUND ──────────────────────────────────────────────────────────────
        public async Task<PaymentResponseDto> ProcessRefund(int rentalItemId)
        {
            var item = await _rentalItemRepository.Get(rentalItemId);
            if (item == null)
                throw new NotFoundException("Rental item not found");

            var rental = await _rentalRepository.Get(item.RentalId);
            if (rental == null)
                throw new NotFoundException("Rental not found");

            // Find the original purchase payment for this rental
            var allPayments = await _paymentRepository.GetAllIncluding(p => p.User);
            var purchasePayment = allPayments?
                .FirstOrDefault(p => p.RentalId == rental.Id && p.PaymentType == PaymentType.Purchase);

            if (purchasePayment == null)
                throw new NotFoundException("Purchase payment not found for this rental");

            if (purchasePayment.Status != PaymentStatus.Success)
                throw new ConflictException("Payment is not eligible for refund");

            // Check if a refund record already exists for this rental item
            var existingRefund = await _context.RentalItemRefunds
                .AnyAsync(r => r.RentalItemId == rentalItemId);
            if (existingRefund)
                throw new ConflictException("Refund already processed for this rental item");

            var now = IstDateTime.Now;
            var hoursSincePurchase = (now - purchasePayment.PaymentDate).TotalHours;

            double refundPct = hoursSincePurchase <= 2 ? 0.75
                             : hoursSincePurchase <= 4 ? 0.50
                             : 0.0;

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

            // Only create a refund payment record if there's an actual amount to refund
            Payment addedRefund;
            if (refundAmount > 0)
            {
                var refundPaymentId = GeneratePaymentId();
                var refundPayment = new Payment
                {
                    RentalId = rental.Id,
                    PaymentId = refundPaymentId,
                    UserId = rental.UserId,
                    Amount = (float)refundAmount,
                    PaymentMethod = purchasePayment.PaymentMethod,
                    PaymentType = PaymentType.Refund,
                    Status = PaymentStatus.Refunded,
                    PaymentDate = now,
                    RefundAmount = refundAmount,
                    RefundedAt = now
                };
                addedRefund = await _paymentRepository.Add(refundPayment);
            }
            else
            {
                // No refund — return the original purchase record as context
                addedRefund = purchasePayment;
            }

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

            await _activityLog.Log(rental.UserId, purchasePayment.User?.Name ?? purchasePayment.User?.Username ?? "Customer", "Customer",
                "Payment", "ProcessRefund",
                $"Refund for rental item #{rentalItemId} (Movie: \"{movie?.Title ?? "Unknown"}\"). Refund: ₹{refundAmount:F2}.");

            return MapToResponse(addedRefund);
        }

        // ── QUERIES ─────────────────────────────────────────────────────────────
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

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByRental(int rentalId)
        {
            var rental = await _rentalRepository.Get(rentalId);
            if (rental == null)
                throw new NotFoundException("Rental not found");

            var payments = await _paymentRepository.GetAllIncluding(p => p.User);
            var result = payments?
                .Where(p => p.RentalId == rentalId)
                .OrderBy(p => p.PaymentDate)
                .ToList();

            if (result == null || !result.Any())
                throw new NotFoundException("No payments found for this rental");

            return result.Select(MapToResponse);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentByUser(int userId)
        {
            var payments = await _paymentRepository.GetAllIncluding(p => p.User);
            var userPayments = payments?
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
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

            return payments
                .OrderByDescending(p => p.PaymentDate)
                .Select(MapToResponse);
        }

        // ── HELPERS ─────────────────────────────────────────────────────────────
        private PaymentResponseDto MapToResponse(Payment p)
        {
            return new PaymentResponseDto
            {
                Id = p.Id,
                RentalId = p.RentalId,
                Amount = p.Amount,
                Method = p.PaymentMethod,
                Status = p.Status,
                PaymentType = p.PaymentType,
                PaymentDate = p.PaymentDate,
                UserId = p.UserId,
                UserName = p.User?.Name ?? "N/A",
                PaymentId = p.PaymentId,
                RefundAmount = p.RefundAmount,
                RefundedAt = p.RefundedAt
            };
        }

        private string GeneratePaymentId()
        {
            return "PAY_" + Guid.NewGuid().ToString("N")[..16].ToUpper();
        }
    }
}
