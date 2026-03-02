using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRepository<int, Rental> _rentalRepository;
        private readonly IRepository<int, RentalItem> _rentalItemRepository;
        private readonly IRepository<int, Inventory> _inventoryRepository;
        private readonly IRepository<int, User> _userRepository;
        private readonly IRepository<int, Movie> _movieRepository;

        public RentalService(
            IRepository<int, Rental> rentalRepository,
            IRepository<int, RentalItem> rentalItemRepository,
            IRepository<int, Inventory> inventoryRepository,
            IRepository<int,Movie> movieRepository,
            IRepository<int, User> userRepository)
        {
            _rentalRepository = rentalRepository;
            _rentalItemRepository = rentalItemRepository;
            _inventoryRepository = inventoryRepository;
            _userRepository = userRepository;
            _movieRepository= movieRepository;
        }

        public async Task<RentalResponseDto> CreateRental(CreateRentalRequestDto request)
        {
            // 1️⃣ Create Rental FIRST
            var rental = new Rental
            {
                UserId = request.UserId,
                RentalDate = DateTime.UtcNow,
                Status = "Active",
                TotalAmount = 0
            };

            var addedRental = await _rentalRepository.Add(rental);

            float totalAmount = 0;

            // 2️⃣ Create RentalItems
            foreach (var movieId in request.MovieIds)
            {
                var inventoryList = await _inventoryRepository.GetAll();

                var inventory = inventoryList
                    ?.FirstOrDefault(i => i.MovieId == movieId && i.IsAvailable);

                if (inventory == null)
                    throw new Exception($"Inventory not available for movie {movieId}");

                var rentalItem = new RentalItem
                {
                    RentalId = addedRental.Id,
                    MovieId = movieId,
                    InventoryId = inventory.Id,        // ⭐ REQUIRED
                    PricePerDay = inventory.RentalPrice,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(request.RentalDays),
                    IsActive = true
                };

                await _rentalItemRepository.Add(rentalItem);

                // ⭐ Add to total amount
                totalAmount += inventory.RentalPrice * request.RentalDays;

                await _inventoryRepository.Update(inventory.Id, inventory);

                // ⭐ Increment movie rental count
                var movie = await _movieRepository.Get(movieId);

                if (movie != null)
                {
                    movie.RentalCount++;
                    await _movieRepository.Update(movie.Id, movie);
                }
            }

            // 3️⃣ Update Total Amount
            addedRental.TotalAmount = totalAmount;
            await _rentalRepository.Update(addedRental.Id, addedRental);

            return new RentalResponseDto
            {
                Id = addedRental.Id,
                UserId = addedRental.UserId,
                RentalDate = addedRental.RentalDate,
                Status = addedRental.Status,
                TotalAmount = addedRental.TotalAmount
            };
        }

        public async Task<IEnumerable<RentalResponseDto>> GetRentalsByUser(int userId)
        {
            var rentals = (await _rentalRepository.GetAll())
                ?.Where(r => r.UserId == userId)
                ?? new List<Rental>();

            return rentals.Select(r => new RentalResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                RentalDate = r.RentalDate,
                Status = r.Status,
                TotalAmount = r.TotalAmount
            });
        }

        public async Task<IEnumerable<RentalItemResponseDto>> GetActiveRentals(int userId)
        {
            await DeactivateExpiredItems();

            var rentals = (await _rentalRepository.GetAll())
                ?.Where(r => r.UserId == userId)
                ?? new List<Rental>();

            var items = (await _rentalItemRepository.GetAll())
                ?.Where(i => rentals.Any(r => r.Id == i.RentalId)
                             && i.IsActive
                             && i.EndDate > DateTime.Now)
                ?? new List<RentalItem>();

            return items.Select(i => new RentalItemResponseDto
            {
                MovieId = i.MovieId,
                PricePerDay = i.PricePerDay,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                IsActive = i.IsActive,
            });
        }
        private async Task DeactivateExpiredItems()
        {
            var items = await _rentalItemRepository.GetAll();

            if (items == null)
                return;

            foreach (var item in items)
            {
                if (item.IsActive && item.EndDate <= DateTime.Now)
                {
                    item.IsActive = false;

                    await _rentalItemRepository.Update(item.Id, item);
                }
            }
        }

        public async Task<bool> EndRentalItem(int rentalItemId)
        {
            var item = await _rentalItemRepository.Get(rentalItemId);

            if (item == null)
                return false;
            if (!item.IsActive)
                return true; 

            item.IsActive = false;

            await _rentalItemRepository.Update(item.Id, item);


            return true;
        }
        
    }
}
