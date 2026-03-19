namespace MovieRentalAPI.Models.DTOs
{
    public class RentalItemRequestDto
    {
        public int MovieId { get; set; }

        public float PricePerDay { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
