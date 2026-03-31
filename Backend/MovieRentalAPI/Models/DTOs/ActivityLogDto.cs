namespace MovieRentalAPI.Models.DTOs
{
    public class ActivityLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
    }

    public class ActivityLogQueryDto
    {
        public int? UserId { get; set; }
        public string? Role { get; set; }       // Customer | Admin | System
        public string? Entity { get; set; }
        public string? Action { get; set; }
        public string? Status { get; set; }     // Success | Failure
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string SortOrder { get; set; } = "desc"; // asc | desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class PagedActivityLogDto
    {
        public IEnumerable<ActivityLogDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
