namespace MovieRentalAPI.Models.DTOs
{
    public class SeasonReviewRequestDto
    {
        public int UserId { get; set; }
        public int SeasonId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class SeasonReviewResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int SeasonId { get; set; }
        public int SeasonNumber { get; set; }
        public string SeasonTitle { get; set; } = string.Empty;
        public string SeriesTitle { get; set; } = string.Empty;
        public int SeriesId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
    }
}
