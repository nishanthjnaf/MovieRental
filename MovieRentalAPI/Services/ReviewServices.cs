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
            var user = await _userRepo.Get(request.UserId);
            var movie = await _movieRepo.Get(request.MovieId);

            if (user == null || movie == null)
                throw new Exception("Invalid User or Movie");

            var review = new Review
            {
                UserId = request.UserId,
                MovieId = request.MovieId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewDate = DateTime.UtcNow
            };

            var added = await _reviewRepo.Add(review);

            return MapToResponse(added);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByMovie(int movieId)
        {
            var reviews = await _reviewRepo.GetAll();

            return reviews
                .Where(r => r.MovieId == movieId)
                .Select(MapToResponse);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByUser(int userId)
        {
            var reviews = await _reviewRepo.GetAll();

            return reviews
                .Where(r => r.UserId == userId)
                .Select(MapToResponse);
        }

        public async Task<ReviewResponseDto?> UpdateReview(int id, ReviewRequestDto request)
        {
            var existing = await _reviewRepo.Get(id);

            if (existing == null)
                return null;

            existing.Rating = request.Rating;
            existing.Comment = request.Comment;

            var updated = await _reviewRepo.Update(id, existing);

            return updated == null ? null : MapToResponse(updated);
        }

        public async Task<bool> DeleteReview(int id)
        {
            var deleted = await _reviewRepo.Delete(id);
            return deleted != null;
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
