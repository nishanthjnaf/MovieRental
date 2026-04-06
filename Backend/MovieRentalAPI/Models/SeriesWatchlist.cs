namespace MovieRentalAPI.Models
{
    public class SeriesWatchlist : IComparable<SeriesWatchlist>, IEquatable<SeriesWatchlist>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SeriesId { get; set; }

        public User? User { get; set; }
        public Series? Series { get; set; }

        public int CompareTo(SeriesWatchlist? other) => other != null ? Id.CompareTo(other.Id) : 1;
        public bool Equals(SeriesWatchlist? other) => other != null && Id == other.Id;
    }
}
