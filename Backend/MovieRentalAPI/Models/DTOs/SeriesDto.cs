namespace MovieRentalAPI.Models.DTOs
{
    public class SeriesRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string? Director { get; set; }
        public string? Cast { get; set; }
        public string? ContentRating { get; set; }
        public string? ContentAdvisory { get; set; }
        public string? PosterPath { get; set; }
        public string? TrailerUrl { get; set; }
        public float RentalPrice { get; set; }
        public bool IsAvailable { get; set; } = true;
        public List<int>? GenreIds { get; set; }
        public List<SeasonRequestDto> Seasons { get; set; } = new();
    }

    public class SeriesResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public List<string> Cast { get; set; } = new();
        public string ContentRating { get; set; } = string.Empty;
        public List<string> ContentAdvisory { get; set; } = new();
        public string? PosterPath { get; set; }
        public string? TrailerUrl { get; set; }
        public float RentalPrice { get; set; }
        public bool IsAvailable { get; set; }
        public int RentalCount { get; set; }
        public List<string> Genres { get; set; } = new();
        public List<SeasonResponseDto> Seasons { get; set; } = new();
    }
}
