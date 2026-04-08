namespace MovieRentalAPI.Models.DTOs
{
    public class RentalItemResponseDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }

        public string? MovieName { get; set; }

        public float PricePerDay { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}
