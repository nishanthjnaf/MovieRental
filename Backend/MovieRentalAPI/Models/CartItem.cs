namespace MovieRentalAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
        public int RentalDays { get; set; } = 7;
        public bool IsRenewal { get; set; } = false;
    }
}
