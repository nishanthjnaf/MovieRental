using MovieRentalAPI.Exceptions;
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
            if (request.MovieId <= 0)
                throw new BadRequestException("Invalid movie id");

            if (request.RentalPrice <= 0)
                throw new BadRequestException("Rental price must be greater than zero");

            var movie = await _movieRepository.Get(request.MovieId);

            if (movie == null)
                throw new NotFoundException("Movie not found");

            var existingInventory = (await _inventoryRepository.GetAll())
                ?.FirstOrDefault(i => i.MovieId == request.MovieId);

            if (existingInventory != null)
                throw new ConflictException("Inventory already exists for this movie");

            var inventory = new Inventory
            {
                MovieId = request.MovieId,
                RentalPrice = request.RentalPrice,
                IsAvailable = request.IsAvailable
            };

            var added = await _inventoryRepository.Add(inventory);

            if (added == null)
                throw new Exception("Inventory creation failed");

            return MapToResponse(added);
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetAllInventory()
        {
            var list = await _inventoryRepository.GetAllIncluding(i => i.Movie);

            if (list == null || !list.Any())
                throw new NotFoundException("No inventory found");

            return list.Select(MapToResponse);
        }

        public async Task<InventoryResponseDto> GetInventoryById(int id)
        {
            var inv = await _inventoryRepository.GetIncluding(id, i => i.Movie);

            if (inv == null)
                throw new NotFoundException("Inventory not found");

            return MapToResponse(inv);
        }

        public async Task<InventoryResponseDto> GetInventoryByMovie(int movieId)
        {
            var movie = await _movieRepository.Get(movieId);

            if (movie == null)
                throw new NotFoundException("Movie not found");

            var list = await _inventoryRepository.GetAll();

            var inv = list?.FirstOrDefault(i => i.MovieId == movieId);

            if (inv == null)
                throw new NotFoundException("Inventory not found for this movie");

            return MapToResponse(inv);
        }

        public async Task<InventoryResponseDto> UpdateInventory(int id, InventoryRequestDto request)
        {
            if (request.RentalPrice <= 0)
                throw new BadRequestException("Rental price must be greater than zero");

            var existing = await _inventoryRepository.Get(id);

            if (existing == null)
                throw new NotFoundException("Inventory not found");

            existing.RentalPrice = request.RentalPrice;
            existing.IsAvailable = request.IsAvailable;

            var updated = await _inventoryRepository.Update(id, existing);

            if (updated == null)
                throw new Exception("Inventory update failed");

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteInventory(int id)
        {
            var existing = await _inventoryRepository.Get(id);

            if (existing == null)
                throw new NotFoundException("Inventory not found");

            var deleted = await _inventoryRepository.Delete(id);

            if (deleted == null)
                throw new Exception("Inventory deletion failed");

            return true;
        }

        public async Task<bool> ToggleAvailability(int id)
        {
            var inv = await _inventoryRepository.Get(id);

            if (inv == null)
                throw new NotFoundException("Inventory not found");

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
                MovieName = inv.Movie.Title,
                IsAvailable = inv.IsAvailable
            };
        }
    }
}