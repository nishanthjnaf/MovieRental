using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRentalAPI.Models
{
    public class Review : IComparable<Review>, IEquatable<Review>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }


        public User? User { get; set; }
        public Movie? Movie { get; set; }

        public int CompareTo(Review? other)
        {
            return other != null ? Id.CompareTo(other.Id) : 1;
        }

        public bool Equals(Review? other)
        {
            return other != null && Id == other.Id;
        }

        public override string ToString()
        {
            return $"Review Id: {Id}, Rating: {Rating}";
        }
    }
}
