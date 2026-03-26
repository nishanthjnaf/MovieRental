using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Helpers;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Models.Enums;

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
            IRepository<int, Movie> movieRepository,
            IRepository<int, User> userRepository)
        {
            _rentalRepository = rentalRepository;
            _rentalItemRepository = rentalItemRepository;
            _inventoryRepository = inventoryRepository;
            _movieRepository = movieRepository;
            _userRepository = userRepository;
        }


        public async Task<RentalResponseDto> CreateRental(CreateRentalRequestDto request)
        {
            var user = await _userRepository.Get(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (request.MovieIds == null || !request.MovieIds.Any())
                throw new BadRequestException("No movies provided");

            var rental = new Rental
            {
                UserId = request.UserId,
                RentalDate = IstDateTime.Now,
                Status = RentalStatus.PaymentPending,
                TotalAmount = 0
            };

            var addedRental = await _rentalRepository.Add(rental);

            float totalAmount = 0;

            for (int idx = 0; idx < request.MovieIds.Count; idx++)
            {
                var movieId = request.MovieIds[idx];
                var days = (request.RentalDaysPerMovie != null && idx < request.RentalDaysPerMovie.Count)
                    ? request.RentalDaysPerMovie[idx]
                    : request.RentalDays;
                if (days <= 0) days = 3;
                if (days < 3) days = 3;

                var movie = await _movieRepository.Get(movieId);
                if (movie == null)
                    throw new NotFoundException($"Movie {movieId} not found");

                var existingItems = await _rentalItemRepository.GetAll();
                var userRentals = (await _rentalRepository.GetAll())
                    ?.Where(r => r.UserId == request.UserId && r.Status == RentalStatus.Available)
                    .Select(r => r.Id)
                    .ToHashSet() ?? new HashSet<int>();

                bool alreadyRented = existingItems?
                    .Any(i =>
                        i.MovieId == movieId &&
                        i.IsActive &&
                        userRentals.Contains(i.RentalId)) ?? false;

                if (alreadyRented)
                    throw new ConflictException("You've already rented this movie");

                var inventoryList = await _inventoryRepository.GetAll();

                var inventory = inventoryList?
                    .FirstOrDefault(i => i.MovieId == movieId && i.IsAvailable);

                if (inventory == null)
                    throw new ConflictException("Movie is currently unavailable");

                var rentalItem = new RentalItem
                {
                    RentalId = addedRental!.Id,
                    MovieId = movieId,
                    InventoryId = inventory.Id,
                    PricePerDay = inventory.RentalPrice,
                    StartDate = IstDateTime.Now,
                    EndDate = IstDateTime.Now.AddDays(days),
                    IsActive = false  // activated only after successful payment
                };

                await _rentalItemRepository.Add(rentalItem);

                await _inventoryRepository.Update(inventory.Id, inventory);

                totalAmount += inventory.RentalPrice * days;

                movie.RentalCount++;
                await _movieRepository.Update(movie.Id, movie);
            }

            addedRental!.TotalAmount = totalAmount;
            await _rentalRepository.Update(addedRental.Id, addedRental);

            return new RentalResponseDto
            {
                Id = addedRental.Id,
                UserId = addedRental.UserId,
                RentalDate = addedRental.RentalDate,
                Status = (PaymentStatus)addedRental.Status,
                TotalAmount = addedRental.TotalAmount
            };
        }
        public async Task<IEnumerable<RentalResponseDto>> GetAllRentals()
        {
            var rentals = await _rentalRepository.GetAll();

            if (rentals == null || !rentals.Any())
                throw new NotFoundException("No rentals found");

            var cutoff = IstDateTime.Now.AddMinutes(-20);
            foreach (var r in rentals)
            {
                if (r.Status == RentalStatus.PaymentPending && r.RentalDate <= cutoff)
                {
                    r.Status = RentalStatus.PaymentNotDone;
                    await _rentalRepository.Update(r.Id, r);
                }
            }

            return rentals.Select(r => new RentalResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                RentalDate = r.RentalDate,
                Status = (PaymentStatus)r.Status,
                TotalAmount = r.TotalAmount
            });
        }


        public async Task<IEnumerable<RentalResponseDto>> GetRentalsByUser(int userId)
        {
            var user = await _userRepository.Get(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            var rentals = (await _rentalRepository.GetAll())
                ?.Where(r => r.UserId == userId)
                .ToList();

            if (rentals == null || rentals.Count == 0)
                throw new NotFoundException("No movie is rented so far");

            return rentals.Select(r => new RentalResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                RentalDate = r.RentalDate,
                Status = (PaymentStatus)r.Status,
                TotalAmount = r.TotalAmount
            });
        }


        public async Task<IEnumerable<RentalItemResponseDto>> GetActiveRentals(int userId)
        {
            var user = await _userRepository.Get(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            await DeactivateExpiredItems();

            var rentals = (await _rentalRepository.GetAll())
                ?.Where(r => r.UserId == userId && r.Status == RentalStatus.Available)
                ?? new List<Rental>();

            var items = (await _rentalItemRepository.GetAll())
                ?.Where(i => rentals.Any(r => r.Id == i.RentalId))
                .ToList();

            if (items == null || items.Count == 0)
                throw new NotFoundException("No active rentals found");

            return items.Select(i => new RentalItemResponseDto
            {
                MovieId = i.MovieId,
                PricePerDay = i.PricePerDay,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                IsActive = i.IsActive,
                Id=i.Id
            });
        }


        public async Task<bool> EndRentalItem(int rentalItemId)
        {
            var item = await _rentalItemRepository.Get(rentalItemId);

            if (item == null)
                throw new NotFoundException("Rental item not found");

            if (!item.IsActive)
                throw new ConflictException("Rental item already ended");

            item.IsActive = false;

            await _rentalItemRepository.Update(item.Id, item);

            return true;
        }

        public async Task<RentalItemResponseDto> RenewRentalItem(
            int rentalItemId,
            RenewRentalRequestDto request)
        {
            if (request == null)
                throw new BadRequestException("Renew request is required");

            if (request.DaysToAdd <= 0)
                throw new BadRequestException("DaysToAdd must be greater than zero");

            var item = await _rentalItemRepository.Get(rentalItemId);
            if (item == null)
                throw new NotFoundException("Rental item not found");

            var utcNow = IstDateTime.Now;

            var wasExpired = item.EndDate <= utcNow;

            // If already expired, renew starting from now; otherwise extend from existing EndDate.
            var baseDate = wasExpired ? utcNow : item.EndDate;
            item.EndDate = baseDate.AddDays(request.DaysToAdd);

            // Align start to now for expired renewals so UI calculations stay intuitive.
            if (wasExpired)
                item.StartDate = utcNow;

            item.IsActive = true;

            await _rentalItemRepository.Update(item.Id, item);

            return new RentalItemResponseDto
            {
                Id = item.Id,
                MovieId = item.MovieId,
                PricePerDay = item.PricePerDay,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                IsActive = item.IsActive
            };
        }


        private async Task DeactivateExpiredItems()
        {
            var items = await _rentalItemRepository.GetAll();

            if (items == null) return;

            foreach (var item in items)
            {
                if (item.IsActive && item.EndDate <= IstDateTime.Now)
                {
                    item.IsActive = false;
                    await _rentalItemRepository.Update(item.Id, item);
                }
            }
        }
        public async Task<IEnumerable<RentalItemResponseDto>> GetRentalItemsByRentalId(int rentalId)
        {
            var rental = await _rentalRepository.Get(rentalId);

            if (rental == null)
                throw new NotFoundException("Rental not found");

            var items = (await _rentalItemRepository.GetAll())
                ?.Where(i => i.RentalId == rentalId)
                .ToList();

            if (items == null || items.Count == 0)
                throw new NotFoundException("No items found for this rental");

            return items.Select(i => new RentalItemResponseDto
            {
                MovieId = i.MovieId,
                PricePerDay = i.PricePerDay,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                IsActive = i.IsActive,
                Id=i.Id
            });
        }

    }
}