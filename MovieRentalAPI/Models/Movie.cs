using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRentalAPI.Models
{
    public class Movie : IComparable<Movie>, IEquatable<Movie>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public int DurationMinutes { get; set; }
        public string Language { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int RentalCount { get; set; } = 0;
        public int MyProperty { get; set; }
        public string? PosterPath { get; set; }
        public string? TrailerUrl { get; set; }

        public required List<Genre> Genres { get; set; }


        public ICollection<Inventory>? Inventories { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<Watchlist>? Watchlists { get; set; }

        public int CompareTo(Movie? other)
        {
            return other != null ? Id.CompareTo(other.Id) : 1;
        }

        public bool Equals(Movie? other)
        {
            return other != null && Id == other.Id;
        }
    }
}
