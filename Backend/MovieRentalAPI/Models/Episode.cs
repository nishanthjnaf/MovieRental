namespace MovieRentalAPI.Models
{
    public class Episode : IComparable<Episode>, IEquatable<Episode>
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime AirDate { get; set; }

        public Season? Season { get; set; }

        public int CompareTo(Episode? other) => other != null ? Id.CompareTo(other.Id) : 1;
        public bool Equals(Episode? other) => other != null && Id == other.Id;
    }
}
