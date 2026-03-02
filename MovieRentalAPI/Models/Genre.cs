using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace MovieRentalAPI.Models
{
    public class Genre : IComparable<Genre>, IEquatable<Genre>
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; }= String.Empty;
        public ICollection<Movie>? Movies { get; set; }

        public int CompareTo(Genre? other)
        {
            return other != null ? Id.CompareTo(other.Id):1;
        }

        public bool Equals(Genre? other)
        {
            return other!=null && Id==other.Id;
        }
        public override string ToString()
        {
            return $"Genre Id: {Id}, Genre Name: {Name}";
        }
    }
}
