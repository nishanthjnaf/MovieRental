namespace MovieRentalAPI.Models
{
    public class SeriesCartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int SeriesId { get; set; }
        public Series? Series { get; set; }
        public int RentalDays { get; set; } = 7;
    }
}
