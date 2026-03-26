namespace MovieRentalAPI.Models.DTOs
{
    public class RentalItemRefundDto
    {
        public int RentalItemId { get; set; }
        public double RefundAmount { get; set; }
        public DateTime RefundedAt { get; set; }
    }
}
