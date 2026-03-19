using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRentalAPI.Models
{
    public class Inventory : IComparable<Inventory>, IEquatable<Inventory>
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public float RentalPrice { get; set; }

        public bool IsAvailable { get; set; }
        public List<RentalItem>? RentalItems { get; internal set; }

        public int CompareTo(Inventory? other)
        {
            return other != null ? Id.CompareTo(other.Id) : 1;
        }

        public bool Equals(Inventory? other)
        {
            return other != null && Id == other.Id;
        }

        public override string ToString()
        {
            return $"Inventory Id: {Id}, MovieId: {MovieId}, Availablity: {IsAvailable}";
        }
    }
}
