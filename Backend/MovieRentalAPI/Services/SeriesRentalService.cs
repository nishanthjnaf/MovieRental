using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Helpers;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Models.Enums;
using MovieRentalModels;

namespace MovieRentalAPI.Services
{
    public class SeriesRentalService : ISeriesRentalService
    {
        private readonly MovieRentalContext _context;

        public SeriesRentalService(MovieRentalContext context)
        {
            _context = context;
        }

        public async Task<RentalResponseDto> CreateSeriesRental(CreateSeriesRentalRequestDto request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) throw new NotFoundException("User not found");

            var series = await _context.Series.FindAsync(request.SeriesId);
            if (series == null) throw new NotFoundException("Series not found");

            if (!series.IsAvailable) throw new ConflictException("Series is currently unavailable");

            var days = Math.Max(3, request.RentalDays);

            // Check already rented
            var alreadyRented = await _context.SeriesRentalItems
                .AnyAsync(r => r.SeriesId == request.SeriesId && r.IsActive &&
                    _context.Rentals.Any(rental => rental.Id == r.RentalId &&
                        rental.UserId == request.UserId &&
                        rental.Status == RentalStatus.Available));

            if (alreadyRented) throw new ConflictException("You have already rented this series");

            var rental = new Rental
            {
                UserId = request.UserId,
                RentalDate = IstDateTime.Now,
                Status = RentalStatus.PaymentPending,
                TotalAmount = series.RentalPrice * days
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            var rentalItem = new SeriesRentalItem
            {
                RentalId = rental.Id,
                SeriesId = request.SeriesId,
                PricePerDay = series.RentalPrice,
                StartDate = IstDateTime.Now,
                EndDate = IstDateTime.Now.AddDays(days),
                IsActive = false
            };

            _context.SeriesRentalItems.Add(rentalItem);
            await _context.SaveChangesAsync();

            return new RentalResponseDto
            {
                Id = rental.Id,
                UserId = rental.UserId,
                RentalDate = rental.RentalDate,
                TotalAmount = rental.TotalAmount
            };
        }

        public async Task<IEnumerable<SeriesRentalItemResponseDto>> GetSeriesRentalsByUser(int userId)
        {
            var rentals = await _context.Rentals
                .Where(r => r.UserId == userId && r.Status == RentalStatus.Available)
                .ToListAsync();

            var rentalIds = rentals.Select(r => r.Id).ToHashSet();

            var items = await _context.SeriesRentalItems
                .Include(r => r.Series)
                .Where(r => rentalIds.Contains(r.RentalId))
                .ToListAsync();

            var result = new List<SeriesRentalItemResponseDto>();
            foreach (var item in items)
            {
                if (item.IsActive && item.EndDate <= IstDateTime.Now)
                {
                    item.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                var rental = rentals.First(r => r.Id == item.RentalId);
                var days = Math.Max(1, (int)Math.Round((item.EndDate - item.StartDate).TotalDays));

                result.Add(new SeriesRentalItemResponseDto
                {
                    Id = item.Id,
                    SeriesId = item.SeriesId,
                    SeriesTitle = item.Series?.Title ?? "",
                    PosterPath = item.Series?.PosterPath,
                    PricePerDay = item.PricePerDay,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    IsActive = item.IsActive,
                    RentalDays = days,
                    TotalAmount = item.PricePerDay * days,
                    RentalId = item.RentalId,
                    RentalStatus = rental.Status
                });
            }

            return result;
        }

        public async Task<bool> EndSeriesRentalItem(int seriesRentalItemId)
        {
            var item = await _context.SeriesRentalItems.FindAsync(seriesRentalItemId);
            if (item == null) throw new NotFoundException("Series rental item not found");

            item.IsActive = false;
            item.EndDate = IstDateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SeriesRentalItemResponseDto> RenewSeriesRentalItem(int seriesRentalItemId, RenewRentalRequestDto request)
        {
            var item = await _context.SeriesRentalItems
                .Include(r => r.Series)
                .FirstOrDefaultAsync(r => r.Id == seriesRentalItemId);

            if (item == null) throw new NotFoundException("Series rental item not found");

            var days = Math.Max(1, request.DaysToAdd);
            item.EndDate = (item.EndDate < IstDateTime.Now ? IstDateTime.Now : item.EndDate).AddDays(days);
            item.IsActive = true;

            await _context.SaveChangesAsync();

            var rentalDays = Math.Max(1, (int)Math.Round((item.EndDate - item.StartDate).TotalDays));
            return new SeriesRentalItemResponseDto
            {
                Id = item.Id,
                SeriesId = item.SeriesId,
                SeriesTitle = item.Series?.Title ?? "",
                PosterPath = item.Series?.PosterPath,
                PricePerDay = item.PricePerDay,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                IsActive = item.IsActive,
                RentalDays = rentalDays,
                TotalAmount = item.PricePerDay * rentalDays,
                RentalId = item.RentalId
            };
        }
    }
}
