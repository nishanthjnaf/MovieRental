using MovieRentalAPI.Models.Enums;

namespace MovieRentalAPI.Models.DTOs
{
    public class RentalResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public DateTime RentalDate { get; set; }

        public PaymentStatus Status { get; set; }

        public float TotalAmount { get; set; }
    }
}