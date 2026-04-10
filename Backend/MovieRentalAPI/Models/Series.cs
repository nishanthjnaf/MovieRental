using System.Collections.Generic;

namespace MovieRentalAPI.Models
{
    public class Series : IComparable<Series>, IEquatable<Series>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Cast { get; set; } = string.Empty;
        public string ContentRating { get; set; } = string.Empty;
        public string ContentAdvisory { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public string? LandscapePosterPath { get; set; }
        public string? TrailerUrl { get; set; }
        public int RentalCount { get; set; } = 0;
        public float RentalPrice { get; set; } = 0;
        public bool IsAvailable { get; set; } = true;

        public required List<Genre> Genres { get; set; }
        public ICollection<Season>? Seasons { get; set; }
        public ICollection<SeriesWatchlist>? Watchlists { get; set; }

        public int CompareTo(Series? other) => other != null ? Id.CompareTo(other.Id) : 1;
        public bool Equals(Series? other) => other != null && Id == other.Id;
    }
}
