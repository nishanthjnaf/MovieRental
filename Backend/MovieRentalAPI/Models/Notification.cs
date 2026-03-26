namespace MovieRentalAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // new_movie | payment | expiry | rate_movie | refund | password | admin_message
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? RelatedId { get; set; } // movieId, rentalItemId, etc.
    }
}
