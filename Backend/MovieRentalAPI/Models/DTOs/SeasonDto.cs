namespace MovieRentalAPI.Models.DTOs
{
    public class SeasonRequestDto
    {
        public int SeasonNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public List<EpisodeRequestDto> Episodes { get; set; } = new();
    }

    public class SeasonResponseDto
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public double AverageRating { get; set; }
        public bool IsNewSeason { get; set; }
        public List<EpisodeResponseDto> Episodes { get; set; } = new();
    }

    // For adding a new season to an existing series
    public class AddSeasonRequestDto
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public List<EpisodeRequestDto> Episodes { get; set; } = new();
    }

    // For adding a new episode to an existing season
    public class AddEpisodeRequestDto
    {
        public int SeasonId { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime? AirDate { get; set; }
    }
}
