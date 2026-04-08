namespace MovieRentalAPI.Models.DTOs
{
    /// <summary>
    /// V2 enriched movie detail — adds live stats on top of the V1 fields.
    /// </summary>
    public class MovieDetailV2ResponseDto
    {
        // ── V1 fields (same as CreateMovieResponseDto) ──────────────────────
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public int DurationMinutes { get; set; }
        public string Language { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Director { get; set; } = string.Empty;
        public List<string> Cast { get; set; } = new();
        public string ContentRating { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
        public List<string> ContentAdvisory { get; set; } = new();
        public string? PosterPath { get; set; }
        public string? LandscapePosterPath { get; set; }
        public string? TrailerUrl { get; set; }

        // ── V2 enriched fields ───────────────────────────────────────────────
        public double AverageUserRating { get; set; }   // avg from Reviews table
        public int TotalRentals { get; set; }            // count from RentalItems table
        public bool IsAvailable { get; set; }            // any active inventory copy
    }
}
