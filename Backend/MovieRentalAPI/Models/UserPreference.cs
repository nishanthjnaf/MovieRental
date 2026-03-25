namespace MovieRentalAPI.Models
{
    public class UserPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        // Comma-separated genre names e.g. "Action,Comedy"
        public string PreferredGenres { get; set; } = string.Empty;

        // Comma-separated language names e.g. "English,Hindi"
        public string PreferredLanguages { get; set; } = string.Empty;

        // "dark" or "light"
        public string Theme { get; set; } = "dark";

        public bool IsSet { get; set; } = false;
    }
}
