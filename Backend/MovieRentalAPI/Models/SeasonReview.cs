namespace MovieRentalAPI.Models
{
    public class SeasonReview : IComparable<SeasonReview>, IEquatable<SeasonReview>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SeasonId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }

        public User? User { get; set; }
        public Season? Season { get; set; }

        public int CompareTo(SeasonReview? other) => other != null ? Id.CompareTo(other.Id) : 1;
        public bool Equals(SeasonReview? other) => other != null && Id == other.Id;
    }
}
