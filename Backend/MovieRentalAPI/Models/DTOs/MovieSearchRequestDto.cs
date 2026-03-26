namespace MovieRentalAPI.Models.DTOs
{
    public class MovieSearchRequestDto
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>Full filter DTO used by GET /api/Movie/filter</summary>
    public class MovieFilterRequestDto
    {
        public string? SearchTerm { get; set; }
        public List<int>? GenreIds { get; set; }
        public List<string>? Languages { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
