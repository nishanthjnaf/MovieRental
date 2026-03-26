namespace MovieRentalAPI.Models.DTOs
{
    public class BroadcastMessageDto
    {
        public int Id { get; set; }
        public int SentByUserId { get; set; }
        public string SentByUsername { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }

    public class SendBroadcastRequestDto
    {
        public int SentByUserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
