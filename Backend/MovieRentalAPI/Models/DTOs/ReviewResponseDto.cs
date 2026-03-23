namespace MovieRentalAPI.Models.DTOs
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty; 

        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
    }
}
