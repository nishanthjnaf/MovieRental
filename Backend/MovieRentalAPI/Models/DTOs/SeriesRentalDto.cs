using MovieRentalAPI.Models.Enums;

namespace MovieRentalAPI.Models.DTOs
{
    public class CreateSeriesRentalRequestDto
    {
        public int UserId { get; set; }
        public int SeriesId { get; set; }
        public int RentalDays { get; set; } = 7;
    }

    public class SeriesRentalItemResponseDto
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public float PricePerDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int RentalDays { get; set; }
        public float TotalAmount { get; set; }
        public int RentalId { get; set; }
        public RentalStatus RentalStatus { get; set; }
    }
}
