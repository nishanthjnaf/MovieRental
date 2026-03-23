namespace MovieRentalAPI.Models.DTOs
{
    public class ReviewRequestDto
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
