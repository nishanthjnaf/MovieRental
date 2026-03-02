namespace MovieRentalAPI.Models.DTOs
{
    public class InventoryResponseDto
    {
        public int Id { get; set; }

        public int MovieId { get; set; }

        public float RentalPrice { get; set; }

        public bool IsAvailable { get; set; }
    }
}