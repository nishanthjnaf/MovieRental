namespace MovieRentalAPI.Models.DTOs
{
    public class WatchlistResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }

        public string MovieTitle { get; set; } = string.Empty;
    }
}
