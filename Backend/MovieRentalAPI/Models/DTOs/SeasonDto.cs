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
        public List<EpisodeResponseDto> Episodes { get; set; } = new();
    }
}
