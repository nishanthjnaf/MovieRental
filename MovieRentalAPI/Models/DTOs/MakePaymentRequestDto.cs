namespace MovieRentalAPI.Models.DTOs
{
    public class MakePaymentRequestDto
    {
        public int RentalId { get; set; }


        public string Method { get; set; } = string.Empty;
    }
}
