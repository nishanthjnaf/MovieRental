using System;
using System.Collections.Generic;
using System.Text;

namespace MovieRentalAPI.Models
{
    public class RentalItem : IComparable<RentalItem>, IEquatable<RentalItem>
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public int MovieId { get; set; }
        public float PricePerDay { get; set; }
        public int InventoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public Rental? Rental { get; set; }

        public int CompareTo(RentalItem? other)
        {
            return other != null ? Id.CompareTo(other.Id) : 1;
        }

        public bool Equals(RentalItem? other)
        {
            return other != null && Id == other.Id;
        }

        public override string ToString()
        {
            return $"Item Id: {Id}, RentalId: {RentalId}, MovieId: {MovieId}";
        }
    }
}
