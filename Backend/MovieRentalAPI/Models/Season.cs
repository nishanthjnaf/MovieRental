namespace MovieRentalAPI.Models
{
    public class Season : IComparable<Season>, IEquatable<Season>
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public double AverageRating { get; set; } = 0;

        public Series? Series { get; set; }
        public ICollection<Episode>? Episodes { get; set; }
        public ICollection<SeasonReview>? Reviews { get; set; }

        public int CompareTo(Season? other) => other != null ? Id.CompareTo(other.Id) : 1;
        public bool Equals(Season? other) => other != null && Id == other.Id;
    }
}
