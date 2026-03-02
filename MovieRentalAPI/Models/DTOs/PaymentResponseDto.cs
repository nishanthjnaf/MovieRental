namespace MovieRentalAPI.Models.DTOs
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }

        public int RentalId { get; set; }
        public int UserId { get; set; }

        public float Amount { get; set; }

        public string Method { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; }
    }
}