namespace MovieRentalAPI.Models
{
    public class BroadcastMessage
    {
        public int Id { get; set; }
        public int SentByUserId { get; set; }
        public string SentByUsername { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
