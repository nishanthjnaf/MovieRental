namespace MovieRentalAPI.Models.DTOs
{
    public class CreateMovieRequestDto
    {
        public string Title { get; set; }=String.Empty;
        public string Description { get; set; }= String.Empty;
        public int ReleaseYear { get; set; }
        public int DurationMinutes { get; set; }
        public string Language { get; set; } = String.Empty;
        public List<int>? GenreIds { get; set; }
        public string? PosterPath { get; set; }
        public string? LandscapePosterPath { get; set; }
        public string? TrailerUrl { get; set; }

        // Movie metadata required by the customer/detail UI
        public string? Director { get; set; }
        // Comma-separated cast names
        public string? Cast { get; set; }
        public string? ContentRating { get; set; }
        // Comma-separated advisory tags
        public string? ContentAdvisory { get; set; }

    }
}
