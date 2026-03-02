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
            var rental = await _rentalRepository.Get(request.RentalId);

            if (rental == null)
                throw new Exception("Rental not found");

            var existingPayments = await _paymentRepository.GetAll();
            if (existingPayments?.Any(p => p.RentalId == request.RentalId) == true)
                throw new Exception("Payment already completed for this rental");

            var payment = new Payment
            {
                RentalId = rental.Id,
                Amount = rental.TotalAmount,
                PaymentMethod = request.Method,
                Status = "Success",
                PaymentDate = DateTime.Now,
                UserId=rental.UserId
            };

            var added = await _paymentRepository.Add(payment);

            rental.Status = "Completed";
            await _rentalRepository.Update(rental.Id, rental);

            return MapToResponse(added);
        }

        public async Task<PaymentResponseDto?> GetPaymentByRental(int rentalId)
        {
            var payments = await _paymentRepository.GetAll();

            var payment = payments?.FirstOrDefault(p => p.RentalId == rentalId);

            return payment == null ? null : MapToResponse(payment);
        }
        public async Task<PaymentResponseDto?> GetPaymentByUser(int userId)
        {
            var payments = await _paymentRepository.GetAll();

            var payment = payments?.FirstOrDefault(p => p.UserId == userId);

            return payment == null ? null : MapToResponse(payment);
        }



        public async Task<IEnumerable<PaymentResponseDto>> GetAllPayments()
        {
            var payments = await _paymentRepository.GetAll();

            if (payments == null)
                return new List<PaymentResponseDto>();

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
                UserId=p.UserId
            };
        }
    }
}
