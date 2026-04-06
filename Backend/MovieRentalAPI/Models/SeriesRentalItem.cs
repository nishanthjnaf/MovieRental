namespace MovieRentalAPI.Models
{
    public class SeriesRentalItem : IComparable<SeriesRentalItem>, IEquatable<SeriesRentalItem>
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public int SeriesId { get; set; }
        public float PricePerDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        public Rental? Rental { get; set; }
        public Series? Series { get; set; }

        public int CompareTo(SeriesRentalItem? other) => other != null ? Id.CompareTo(other.Id) : 1;
        public bool Equals(SeriesRentalItem? other) => other != null && Id == other.Id;
    }
}
