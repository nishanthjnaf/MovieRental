namespace MovieRentalAPI.Models.DTOs
{
    public class CreateMovieResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int ReleaseYear { get; set; }
        public int DurationMinutes { get; set; }
        public string Language { get; set; } = String.Empty;
        public double Rating { get; set; }
        public string Director { get; set; } = String.Empty;
        public List<string> Cast { get; set; } = new List<string>();
        public string ContentRating { get; set; } = String.Empty;
        public List<string> Genres { get; set; } = new List<string>();
        public List<string> ContentAdvisory { get; set; } = new List<string>();
        public string? PosterPath { get; set; }
        public string? TrailerUrl { get; set; }
        public int RentalCount { get; set; }
    }
}
