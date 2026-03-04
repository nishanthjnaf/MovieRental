using MovieRentalAPI.Models.Enums;

namespace MovieRentalAPI.Models.DTOs
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }

        public int RentalId { get; set; }
        public int UserId { get; set; }

        public float Amount { get; set; }

        public PaymentMethod Method { get; set; } 

        public PaymentStatus Status { get; set; } 

        public DateTime PaymentDate { get; set; }
        public string PaymentId { get; internal set; }
    }
}