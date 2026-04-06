using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalModels;

namespace MovieRentalAPI.Services
{
    public class SeriesWatchlistService : ISeriesWatchlistService
    {
        private readonly MovieRentalContext _context;

        public SeriesWatchlistService(MovieRentalContext context)
        {
            _context = context;
        }

        public async Task<SeriesWatchlistResponseDto> AddToWatchlist(SeriesWatchlistRequestDto request)
        {
            if (request.UserId <= 0 || request.SeriesId <= 0)
                throw new BadRequestException("Invalid user or series id");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) throw new NotFoundException("User not found");

            var series = await _context.Series.FindAsync(request.SeriesId);
            if (series == null) throw new NotFoundException("Series not found");

            var exists = await _context.SeriesWatchlists
                .AnyAsync(w => w.UserId == request.UserId && w.SeriesId == request.SeriesId);
            if (exists) throw new ConflictException("Series already in watchlist");

            var item = new SeriesWatchlist { UserId = request.UserId, SeriesId = request.SeriesId };
            _context.SeriesWatchlists.Add(item);
            await _context.SaveChangesAsync();

            return new SeriesWatchlistResponseDto
            {
                Id = item.Id,
                UserId = item.UserId,
                SeriesId = item.SeriesId,
                SeriesTitle = series.Title
            };
        }

        public async Task<IEnumerable<SeriesWatchlistResponseDto>> GetUserWatchlist(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new NotFoundException("User not found");

            var items = await _context.SeriesWatchlists
                .Include(w => w.Series)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return items.Select(w => new SeriesWatchlistResponseDto
            {
                Id = w.Id,
                UserId = w.UserId,
                SeriesId = w.SeriesId,
                SeriesTitle = w.Series?.Title ?? ""
            });
        }

        public async Task<bool> RemoveFromWatchlist(int id)
        {
            var item = await _context.SeriesWatchlists.FindAsync(id);
            if (item == null) throw new NotFoundException("Watchlist item not found");

            _context.SeriesWatchlists.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
