namespace MovieRentalAPI.Models.DTOs
{
    public class TokenPayloadDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
