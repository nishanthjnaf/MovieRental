using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> MakePayment(MakePaymentRequestDto request);

        Task<PaymentResponseDto?> GetPaymentByRental(int rentalId);
        Task<IEnumerable<PaymentResponseDto?>> GetPaymentByUser(int userId);


        Task<IEnumerable<PaymentResponseDto>> GetAllPayments();
    }
}
