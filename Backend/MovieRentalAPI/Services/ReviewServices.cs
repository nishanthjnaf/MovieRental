using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Helpers;
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
        private readonly IRepository<int, Rental> _rentalRepo;
        private readonly IRepository<int, RentalItem> _rentalItemRepo;

        public ReviewService(
            IRepository<int, Review> reviewRepo,
            IRepository<int, Movie> movieRepo,
            IRepository<int, User> userRepo,
            IRepository<int, Rental> rentalRepo,
            IRepository<int, RentalItem> rentalItemRepo)
        {
            _reviewRepo = reviewRepo;
            _movieRepo = movieRepo;
            _userRepo = userRepo;
            _rentalRepo = rentalRepo;
            _rentalItemRepo = rentalItemRepo;
        }

        public async Task<ReviewResponseDto> AddReview(ReviewRequestDto request)
        {
            if (request.Rating < 0 || request.Rating > 10)
                throw new BadRequestException("Rating must be between 0 and 10");

            var user = await _userRepo.Get(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            var movie = await _movieRepo.Get(request.MovieId);
            if (movie == null)
                throw new NotFoundException("Movie not found");

            var rentals = (await _rentalRepo.GetAll())
                ?.Where(r => r.UserId == request.UserId)
                .Select(r => r.Id)
                .ToHashSet() ?? new HashSet<int>();

            var rentalItems = await _rentalItemRepo.GetAll();
            var hasRentedMovie = rentalItems?.Any(ri =>
                rentals.Contains(ri.RentalId) &&
                ri.MovieId == request.MovieId) ?? false;

            if (!hasRentedMovie)
                throw new BadRequestException("You can rate only movies you have rented");

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
                ReviewDate = IstDateTime.Now
            };

            var added = await _reviewRepo.Add(review);

            if (added == null)
                throw new Exception("Review creation failed");

            await UpdateMovieRating(request.MovieId);

            return MapToResponse(added);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByMovie(int movieId)
        {
            var movie = await _movieRepo.Get(movieId);
            if (movie == null)
                throw new NotFoundException("Movie not found");

            var reviews = await _reviewRepo
                .GetAllIncluding(r => r.User, r => r.Movie);

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

            var reviews = await _reviewRepo
                .GetAllIncluding(r => r.User, r => r.Movie);

            var userReviews = reviews?
                .Where(r => r.UserId == userId)
                .ToList();

            if (userReviews == null || !userReviews.Any())
                return Enumerable.Empty<ReviewResponseDto>();

            return userReviews.Select(MapToResponse);
        }

        public async Task<ReviewResponseDto> UpdateReview(int id, ReviewRequestDto request)
        {
            if (request.Rating < 0 || request.Rating > 10)
                throw new BadRequestException("Rating must be between 0 and 10");

            var existing = await _reviewRepo.Get(id);

            if (existing == null)
                throw new NotFoundException("Review not found");

            existing.Rating = request.Rating;
            existing.Comment = request.Comment;

            var updated = await _reviewRepo.Update(id, existing);

            if (updated == null)
                throw new Exception("Review update failed");

            await UpdateMovieRating(existing.MovieId);

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteReview(int id)
        {
            var existing = await _reviewRepo.Get(id);

            if (existing == null)
                throw new NotFoundException("Review not found");

            var movieId = existing.MovieId;

            var deleted = await _reviewRepo.Delete(id);

            if (deleted == null)
                throw new Exception("Review deletion failed");

            await UpdateMovieRating(movieId);

            return true;
        }

        private async Task UpdateMovieRating(int movieId)
        {
            var movie = await _movieRepo.Get(movieId);
            if (movie == null)
                throw new NotFoundException("Movie not found");

            var allReviews = await _reviewRepo.GetAll();
            var movieReviews = allReviews?
                .Where(r => r.MovieId == movieId)
                .ToList();

            movie.Rating = movieReviews != null && movieReviews.Any()
                ? movieReviews.Average(r => r.Rating)
                : 0;

            await _movieRepo.Update(movieId, movie);
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
                ReviewDate = r.ReviewDate,
                UserName = r.User != null ? r.User.Username : "N/A",
                MovieName = r.Movie != null ? r.Movie.Title : "N/A"

            };
        }
    }
}