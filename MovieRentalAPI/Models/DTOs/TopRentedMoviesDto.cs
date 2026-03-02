namespace MovieRentalAPI.Models.DTOs
{
    public class TopRentedMovieDto
    {
        public int MovieId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int RentalCount { get; set; }

        public int ReleaseYear { get; set; }

        public string Language { get; set; } = string.Empty;
    }
}
