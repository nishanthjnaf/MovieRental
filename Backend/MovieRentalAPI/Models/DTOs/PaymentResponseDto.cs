using MovieRentalAPI.Models.Enums;

namespace MovieRentalAPI.Models.DTOs
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }

        public int RentalId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public float Amount { get; set; }

        public PaymentMethod Method { get; set; } 

        public PaymentStatus Status { get; set; } 

        public DateTime PaymentDate { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public double? RefundAmount { get; set; }
        public DateTime? RefundedAt { get; set; }

    }
}