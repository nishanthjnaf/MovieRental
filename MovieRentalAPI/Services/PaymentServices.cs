using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

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
            if (request.RentalId <= 0)
                throw new BadRequestException("Invalid rental id");

            if (string.IsNullOrWhiteSpace(request.Method))
                throw new BadRequestException("Payment method is required");

            var rental = await _rentalRepository.Get(request.RentalId);

            if (rental == null)
                throw new NotFoundException("Rental not found");

            if (rental.TotalAmount <= 0)
                throw new BadRequestException("Invalid rental amount");

            if (rental.Status == "Completed")
                throw new ConflictException("Payment already completed for this rental");

            var existingPayments = await _paymentRepository.GetAll();

            if (existingPayments != null &&
                existingPayments.Any(p => p.RentalId == request.RentalId))
                throw new ConflictException("Payment already exists for this rental");

            var payment = new Payment
            {
                RentalId = rental.Id,
                Amount = rental.TotalAmount,
                PaymentMethod = request.Method,
                Status = "Success",
                PaymentDate = DateTime.UtcNow,
                UserId = rental.UserId
            };

            var added = await _paymentRepository.Add(payment);

            if (added == null)
                throw new Exception("Payment failed");

            rental.Status = "Completed";
            await _rentalRepository.Update(rental.Id, rental);

            return MapToResponse(added);
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
    }
}