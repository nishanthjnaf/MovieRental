namespace MovieRentalAPI.Models.DTOs
{
    public class GenreResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; }= String.Empty;
    }
}
