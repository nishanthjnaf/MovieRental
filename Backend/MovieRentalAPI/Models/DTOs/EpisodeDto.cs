namespace MovieRentalAPI.Models.DTOs
{
    public class EpisodeRequestDto
    {
        public int EpisodeNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        // Optional override; if not provided, auto-set to today
        public DateTime? AirDate { get; set; }
    }

    public class EpisodeResponseDto
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime AirDate { get; set; }
    }
}
