using MovieRentalAPI.Models.Enums;

namespace MovieRentalAPI.Models.DTOs
{
    public class MakePaymentRequestDto
    {
        public int RentalId { get; set; }


        public PaymentMethod Method { get; set; }
        public bool IsSuccess { get; set; }
    }
}
