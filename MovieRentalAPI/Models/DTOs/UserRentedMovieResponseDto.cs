namespace MovieRentalAPI.Models.DTOs
{
    public class UserRentedMovieResponseDto
    {
        public int RentalId { get; set; }

        public int RentalItemId { get; set; }

        public int MovieId { get; set; }

        public string MovieTitle { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public float PricePerDay { get; set; }

        public bool IsActive { get; set; }

        public string RentalStatus { get; set; } = string.Empty;
    }
}
