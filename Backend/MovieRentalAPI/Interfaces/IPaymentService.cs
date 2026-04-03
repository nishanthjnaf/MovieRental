using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Models.Enums;

namespace MovieRentalAPI.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> MakePayment(MakePaymentRequestDto request);
        Task<PaymentResponseDto> MakeRenewalPayment(int rentalItemId, int daysToAdd, PaymentMethod method);
        Task<PaymentResponseDto> ProcessRefund(int rentalItemId);
        Task<RentalItemRefundDto?> GetItemRefund(int rentalItemId);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByRental(int rentalId);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentByUser(int userId);
        Task<IEnumerable<PaymentResponseDto>> GetAllPayments();
    }
}
