using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IRepository<int, Inventory> _inventoryRepository;
        private readonly IRepository<int, Movie> _movieRepository;

        public InventoryService(
            IRepository<int, Inventory> inventoryRepository,
            IRepository<int, Movie> movieRepository)
        {
            _inventoryRepository = inventoryRepository;
            _movieRepository = movieRepository;
        }

        public async Task<InventoryResponseDto> AddInventory(InventoryRequestDto request)
        {
            var movie = await _movieRepository.Get(request.MovieId);

            if (movie == null)
                throw new Exception("Movie not found");

            var inventory = new Inventory
            {
                MovieId = request.MovieId,
                Movie = movie,
                RentalPrice = request.RentalPrice,
                IsAvailable = request.IsAvailable
            };

            var added = await _inventoryRepository.Add(inventory);

            return MapToResponse(added);
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetAllInventory()
        {
            var list = await _inventoryRepository.GetAll();

            if (list == null)
                return new List<InventoryResponseDto>();

            return list.Select(MapToResponse);
        }

        public async Task<InventoryResponseDto?> GetInventoryById(int id)
        {
            var inv = await _inventoryRepository.Get(id);

            return inv == null ? null : MapToResponse(inv);
        }

        public async Task<InventoryResponseDto?> GetInventoryByMovie(int movieId)
        {
            var list = await _inventoryRepository.GetAll();

            var inv = list?.FirstOrDefault(i => i.MovieId == movieId);

            return inv == null ? null : MapToResponse(inv);
        }

        public async Task<InventoryResponseDto?> UpdateInventory(int id, InventoryRequestDto request)
        {
            var existing = await _inventoryRepository.Get(id);

            if (existing == null)
                return null;

            existing.RentalPrice = request.RentalPrice;
            existing.IsAvailable = request.IsAvailable;

            var updated = await _inventoryRepository.Update(id, existing);

            return updated == null ? null : MapToResponse(updated);
        }

        public async Task<bool> DeleteInventory(int id)
        {
            var deleted = await _inventoryRepository.Delete(id);
            return deleted != null;
        }

        public async Task<bool> ToggleAvailability(int id)
        {
            var inv = await _inventoryRepository.Get(id);

            if (inv == null)
                return false;

            inv.IsAvailable = !inv.IsAvailable;

            await _inventoryRepository.Update(id, inv);

            return true;
        }

        private InventoryResponseDto MapToResponse(Inventory inv)
        {

            return new InventoryResponseDto
            {
                Id = inv.Id,
                MovieId = inv.MovieId,
                RentalPrice = inv.RentalPrice,
                IsAvailable = inv.IsAvailable
            };
        }
    }
}

