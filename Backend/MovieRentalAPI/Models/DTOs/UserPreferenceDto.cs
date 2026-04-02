namespace MovieRentalAPI.Models.DTOs
{
    public class SavePreferenceRequestDto
    {
        public List<string>? PreferredGenres { get; set; }
        public List<string>? PreferredLanguages { get; set; }
        public string? Theme { get; set; }
    }

    public class UserPreferenceResponseDto
    {
        public List<string> PreferredGenres { get; set; } = new();
        public List<string> PreferredLanguages { get; set; } = new();
        public string Theme { get; set; } = "dark";
        public bool IsSet { get; set; }
    }
}
