namespace MovieRentalAPI.Models.DTOs
{
    public class CreateRentalRequestDto
    {
        public int UserId { get; set; }
        public List<int> MovieIds { get; set; } = new();
        public int RentalDays { get; set; }
        // Per-movie rental days — if provided, overrides RentalDays for each movie by index
        public List<int>? RentalDaysPerMovie { get; set; }
    }
}
