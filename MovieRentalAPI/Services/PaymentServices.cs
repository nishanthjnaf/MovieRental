using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Models.Enums;

namespace MovieRentalAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<int, Payment> _paymentRepository;
        private readonly IRepository<int, Rental> _rentalRepository;

        public PaymentService(
            IRepository<int, Payment> paymentRepository,
            IRepository<int, Rental> rentalRepository)
        {
            _paymentRepository = paymentRepository;
            _rentalRepository = rentalRepository;
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
                PaymentDate = DateTime.UtcNow
            };

            var added = await _paymentRepository.Add(payment);

            rental.Status = rentalStatus;
            rental.PaymentId = paymentId;

            await _rentalRepository.Update(rental.Id, rental);

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

        public async Task<PaymentResponseDto> GetPaymentByRental(int rentalId)
        {
            var rental = await _rentalRepository.Get(rentalId);

            if (rental == null)
                throw new NotFoundException("Rental not found");

            var payments = await _paymentRepository.GetAll();

            var payment = payments?
                .FirstOrDefault(p => p.RentalId == rentalId);

            if (payment == null)
                throw new NotFoundException("Payment not found for this rental");

            return MapToResponse(payment);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentByUser(int userId)
        {
            var payments = await _paymentRepository.GetAll();

            var userPayments = payments?
                .Where(p => p.UserId == userId)
                .ToList();

            if (userPayments == null || !userPayments.Any())
                throw new NotFoundException("No payments found for this user");

            return userPayments.Select(MapToResponse);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPayments()
        {
            var payments = await _paymentRepository.GetAll();

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
                UserId = p.UserId
            };
        }
        private string GeneratePaymentId()
        {
            return "PAY_" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
        }
    }
}