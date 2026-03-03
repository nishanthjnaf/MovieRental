using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepository<int, Review> _reviewRepo;
        private readonly IRepository<int, Movie> _movieRepo;
        private readonly IRepository<int, User> _userRepo;

        public ReviewService(
            IRepository<int, Review> reviewRepo,
            IRepository<int, Movie> movieRepo,
            IRepository<int, User> userRepo)
        {
            _reviewRepo = reviewRepo;
            _movieRepo = movieRepo;
            _userRepo = userRepo;
        }

        public async Task<ReviewResponseDto> AddReview(ReviewRequestDto request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new BadRequestException("Rating must be between 1 and 5");

            var user = await _userRepo.Get(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            var movie = await _movieRepo.Get(request.MovieId);
            if (movie == null)
                throw new NotFoundException("Movie not found");

            var existingReviews = await _reviewRepo.GetAll();

            if (existingReviews != null &&
                existingReviews.Any(r =>
                    r.UserId == request.UserId &&
                    r.MovieId == request.MovieId))
                throw new ConflictException("You have already reviewed this movie");

            var review = new Review
            {
                UserId = request.UserId,
                MovieId = request.MovieId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewDate = DateTime.UtcNow
            };

            var added = await _reviewRepo.Add(review);

            if (added == null)
                throw new Exception("Review creation failed");

            return MapToResponse(added);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByMovie(int movieId)
        {
            var movie = await _movieRepo.Get(movieId);
            if (movie == null)
                throw new NotFoundException("Movie not found");

            var reviews = await _reviewRepo.GetAll();

            var movieReviews = reviews?
                .Where(r => r.MovieId == movieId)
                .ToList();

            if (movieReviews == null || !movieReviews.Any())
                throw new NotFoundException("No reviews found for this movie");

            return movieReviews.Select(MapToResponse);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByUser(int userId)
        {
            var user = await _userRepo.Get(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            var reviews = await _reviewRepo.GetAll();

            var userReviews = reviews?
                .Where(r => r.UserId == userId)
                .ToList();

            if (userReviews == null || !userReviews.Any())
                throw new NotFoundException("No reviews found for this user");

            return userReviews.Select(MapToResponse);
        }

        public async Task<ReviewResponseDto> UpdateReview(int id, ReviewRequestDto request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new BadRequestException("Rating must be between 1 and 5");

            var existing = await _reviewRepo.Get(id);

            if (existing == null)
                throw new NotFoundException("Review not found");

            existing.Rating = request.Rating;
            existing.Comment = request.Comment;

            var updated = await _reviewRepo.Update(id, existing);

            if (updated == null)
                throw new Exception("Review update failed");

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteReview(int id)
        {
            var existing = await _reviewRepo.Get(id);

            if (existing == null)
                throw new NotFoundException("Review not found");

            var deleted = await _reviewRepo.Delete(id);

            if (deleted == null)
                throw new Exception("Review deletion failed");

            return true;
        }

        private ReviewResponseDto MapToResponse(Review r)
        {
            return new ReviewResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                MovieId = r.MovieId,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewDate = r.ReviewDate
            };
        }
    }
}