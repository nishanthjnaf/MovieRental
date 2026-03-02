using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryResponseDto> AddInventory(InventoryRequestDto request);

        Task<IEnumerable<InventoryResponseDto>> GetAllInventory();

        Task<InventoryResponseDto?> GetInventoryById(int id);

        Task<InventoryResponseDto?> GetInventoryByMovie(int movieId);

        Task<InventoryResponseDto?> UpdateInventory(int id, InventoryRequestDto request);

        Task<bool> DeleteInventory(int id);

        Task<bool> ToggleAvailability(int id);
    }
}
