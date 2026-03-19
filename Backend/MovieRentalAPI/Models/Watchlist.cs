using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRentalAPI.Models
{
    public class Watchlist : IComparable<Watchlist>, IEquatable<Watchlist>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }

        public User? User { get; set; }
        public Movie? Movie { get; set; }

        public int CompareTo(Watchlist? other)
        {
            return other != null ? Id.CompareTo(other.Id) : 1;
        }

        public bool Equals(Watchlist? other)
        {
            return other != null && Id == other.Id;
        }

        public override string ToString()
        {
            return $"Watchlist Id: {Id}, MovieId: {MovieId}";
        }
    }
}
