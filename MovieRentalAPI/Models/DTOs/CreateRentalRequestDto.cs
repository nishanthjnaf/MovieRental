namespace MovieRentalAPI.Models.DTOs
{
    public class CreateRentalRequestDto
    {
        public int UserId { get; set; }

        public List<int> MovieIds { get; set; } = new();
        public int RentalDays { get; set; }

    }
}
