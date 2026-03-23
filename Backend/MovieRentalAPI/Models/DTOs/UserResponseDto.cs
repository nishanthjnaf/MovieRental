namespace MovieRentalAPI.Models.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = String.Empty;
        public string Phone { get; set; } = String.Empty;
    }
}
