namespace MovieRentalAPI.Models.DTOs
{
    public class InventoryRequestDto
    {
        public int MovieId { get; set; }

        public float RentalPrice { get; set; }

        public bool IsAvailable { get; set; }
    }
}