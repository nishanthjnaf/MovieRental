namespace MovieRentalAPI.Models.DTOs
{
    public class SeriesWatchlistRequestDto
    {
        public int UserId { get; set; }
        public int SeriesId { get; set; }
    }

    public class SeriesWatchlistResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; } = string.Empty;
    }
}
