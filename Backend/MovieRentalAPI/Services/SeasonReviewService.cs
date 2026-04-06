using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Helpers;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalModels;

namespace MovieRentalAPI.Services
{
    public class SeasonReviewService : ISeasonReviewService
    {
        private readonly MovieRentalContext _context;

        public SeasonReviewService(MovieRentalContext context)
        {
            _context = context;
        }

        public async Task<SeasonReviewResponseDto> AddReview(SeasonReviewRequestDto request)
        {
            if (request.Rating < 0 || request.Rating > 10)
                throw new BadRequestException("Rating must be between 0 and 10");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) throw new NotFoundException("User not found");

            var season = await _context.Seasons.FindAsync(request.SeasonId);
            if (season == null) throw new NotFoundException("Season not found");

            // Check user has rented the series
            var seriesId = season.SeriesId;
            var hasRented = await _context.SeriesRentalItems
                .AnyAsync(r => r.SeriesId == seriesId &&
                    _context.Rentals.Any(rental => rental.Id == r.RentalId && rental.UserId == request.UserId));

            if (!hasRented)
                throw new BadRequestException("You can only rate seasons of series you have rented");

            var exists = await _context.SeasonReviews
                .AnyAsync(r => r.UserId == request.UserId && r.SeasonId == request.SeasonId);
            if (exists)
                throw new ConflictException("You have already reviewed this season");

            var review = new SeasonReview
            {
                UserId = request.UserId,
                SeasonId = request.SeasonId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewDate = IstDateTime.Now
            };

            _context.SeasonReviews.Add(review);
            await _context.SaveChangesAsync();

            await UpdateSeasonRating(request.SeasonId);

            return await MapToDto(review.Id);
        }

        public async Task<IEnumerable<SeasonReviewResponseDto>> GetReviewsBySeason(int seasonId)
        {
            var reviews = await _context.SeasonReviews
                .Include(r => r.User)
                .Include(r => r.Season).ThenInclude(s => s!.Series)
                .Where(r => r.SeasonId == seasonId)
                .ToListAsync();

            return reviews.Select(MapToResponse);
        }

        public async Task<IEnumerable<SeasonReviewResponseDto>> GetReviewsByUser(int userId)
        {
            var reviews = await _context.SeasonReviews
                .Include(r => r.User)
                .Include(r => r.Season).ThenInclude(s => s!.Series)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return reviews.Select(MapToResponse);
        }

        public async Task<bool> DeleteReview(int id)
        {
            var review = await _context.SeasonReviews.FindAsync(id);
            if (review == null) throw new NotFoundException("Review not found");

            var seasonId = review.SeasonId;
            _context.SeasonReviews.Remove(review);
            await _context.SaveChangesAsync();
            await UpdateSeasonRating(seasonId);
            return true;
        }

        private async Task UpdateSeasonRating(int seasonId)
        {
            var season = await _context.Seasons.FindAsync(seasonId);
            if (season == null) return;

            var avg = await _context.SeasonReviews
                .Where(r => r.SeasonId == seasonId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;

            season.AverageRating = avg;
            await _context.SaveChangesAsync();
        }

        private async Task<SeasonReviewResponseDto> MapToDto(int reviewId)
        {
            var r = await _context.SeasonReviews
                .Include(x => x.User)
                .Include(x => x.Season).ThenInclude(s => s!.Series)
                .FirstAsync(x => x.Id == reviewId);

            return MapToResponse(r);
        }

        private static SeasonReviewResponseDto MapToResponse(SeasonReview r) => new()
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User?.Username ?? "N/A",
            SeasonId = r.SeasonId,
            SeasonNumber = r.Season?.SeasonNumber ?? 0,
            SeasonTitle = r.Season?.Title ?? "N/A",
            SeriesTitle = r.Season?.Series?.Title ?? "N/A",
            SeriesId = r.Season?.SeriesId ?? 0,
            Rating = r.Rating,
            Comment = r.Comment,
            ReviewDate = r.ReviewDate
        };
    }
}
