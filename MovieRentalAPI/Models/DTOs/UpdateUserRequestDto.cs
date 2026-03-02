namespace MovieRentalAPI.Models.DTOs
{
    public class UpdateUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = String.Empty;
        public string Phone { get; set; } = String.Empty;
    }
}
