namespace MovieRentalAPI.Models.DTOs
{
    public class RentalResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime RentalDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public float TotalAmount { get; set; }
    }
}