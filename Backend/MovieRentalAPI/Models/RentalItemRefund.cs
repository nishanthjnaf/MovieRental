namespace MovieRentalAPI.Models
{
    public class RentalItemRefund
    {
        public int Id { get; set; }
        public int RentalItemId { get; set; }
        public int RentalId { get; set; }
        public int UserId { get; set; }
        public double RefundAmount { get; set; }
        public DateTime RefundedAt { get; set; }
    }
}
