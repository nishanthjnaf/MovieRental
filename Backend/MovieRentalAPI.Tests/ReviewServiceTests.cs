using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;
using Xunit;

namespace MovieRentalAPI.Tests;

public class ReviewServiceTests
{
    private readonly Mock<IRepository<int, Review>> _reviewRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly Mock<IRepository<int, User>> _userRepo = new();
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _rentalItemRepo = new();
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        _sut = new ReviewService(
            _reviewRepo.Object, _movieRepo.Object, _userRepo.Object,
            _rentalRepo.Object, _rentalItemRepo.Object);
    }

    private static User MakeUser(int id = 1) => new() { Id = id, Username = "u1" };
    private static Movie MakeMovie(int id = 1) => new() { Id = id, Title = "Film", Genres = new List<Genre>() };
    private static Review MakeReview(int id = 1, int userId = 1, int movieId = 1) =>
        new() { Id = id, UserId = userId, MovieId = movieId, Rating = 8, Comment = "Good" };

    private void SetupRentedMovie(int userId = 1, int movieId = 1)
    {
        _rentalRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Rental> { new() { Id = 1, UserId = userId } });
        _rentalItemRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<RentalItem> { new() { Id = 1, RentalId = 1, MovieId = movieId } });
    }

    // ── AddReview ───────────────────────────────────────────────────────────

    [Fact]
    public async Task AddReview_Valid_ReturnsDto()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        SetupRentedMovie();
        _reviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review>());
        _reviewRepo.Setup(r => r.Add(It.IsAny<Review>()))
            .ReturnsAsync((Review rv) => { rv.Id = 1; return rv; });
        _movieRepo.Setup(r => r.Update(1, It.IsAny<Movie>())).ReturnsAsync(MakeMovie());

        var result = await _sut.AddReview(new ReviewRequestDto { UserId = 1, MovieId = 1, Rating = 8, Comment = "Good" });
        Assert.Equal(8, result.Rating);
    }

    [Fact]
    public async Task AddReview_RatingAbove10_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddReview(new ReviewRequestDto { UserId = 1, MovieId = 1, Rating = 11 }));
    }

    [Fact]
    public async Task AddReview_NegativeRating_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddReview(new ReviewRequestDto { UserId = 1, MovieId = 1, Rating = -1 }));
    }

    [Fact]
    public async Task AddReview_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AddReview(new ReviewRequestDto { UserId = 99, MovieId = 1, Rating = 5 }));
    }

    [Fact]
    public async Task AddReview_MovieNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(99)).ReturnsAsync((Movie?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AddReview(new ReviewRequestDto { UserId = 1, MovieId = 99, Rating = 5 }));
    }

    [Fact]
    public async Task AddReview_NotRented_ThrowsBadRequest()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        _rentalItemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddReview(new ReviewRequestDto { UserId = 1, MovieId = 1, Rating = 5 }));
    }

    [Fact]
    public async Task AddReview_AlreadyReviewed_ThrowsConflict()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        SetupRentedMovie();
        _reviewRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Review> { MakeReview() });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.AddReview(new ReviewRequestDto { UserId = 1, MovieId = 1, Rating = 5 }));
    }

    [Fact]
    public async Task AddReview_AddReturnsNull_ThrowsException()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        SetupRentedMovie();
        _reviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review>());
        _reviewRepo.Setup(r => r.Add(It.IsAny<Review>())).ReturnsAsync((Review?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _sut.AddReview(new ReviewRequestDto { UserId = 1, MovieId = 1, Rating = 5 }));
    }

    // ── GetReviewsByMovie ───────────────────────────────────────────────────

    [Fact]
    public async Task GetReviewsByMovie_HasReviews_ReturnsAll()
    {
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _reviewRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Review, object>>[]>()))
            .ReturnsAsync(new List<Review> { MakeReview(), MakeReview(2) });

        var result = (await _sut.GetReviewsByMovie(1)).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetReviewsByMovie_MovieNotFound_ThrowsNotFound()
    {
        _movieRepo.Setup(r => r.Get(99)).ReturnsAsync((Movie?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetReviewsByMovie(99));
    }

    [Fact]
    public async Task GetReviewsByMovie_NoReviews_ThrowsNotFound()
    {
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _reviewRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Review, object>>[]>()))
            .ReturnsAsync(new List<Review>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetReviewsByMovie(1));
    }

    // ── GetReviewsByUser ────────────────────────────────────────────────────

    [Fact]
    public async Task GetReviewsByUser_HasReviews_ReturnsAll()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _reviewRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Review, object>>[]>()))
            .ReturnsAsync(new List<Review> { MakeReview() });

        var result = (await _sut.GetReviewsByUser(1)).ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetReviewsByUser_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetReviewsByUser(99));
    }

    [Fact]
    public async Task GetReviewsByUser_NoReviews_ReturnsEmpty()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _reviewRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Review, object>>[]>()))
            .ReturnsAsync(new List<Review>());

        var result = await _sut.GetReviewsByUser(1);
        Assert.Empty(result);
    }

    // ── UpdateReview ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateReview_Valid_ReturnsUpdated()
    {
        var review = MakeReview();
        _reviewRepo.Setup(r => r.Get(1)).ReturnsAsync(review);
        _reviewRepo.Setup(r => r.Update(1, It.IsAny<Review>()))
            .ReturnsAsync((int _, Review rv) => rv);
        _reviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review> { review });
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _movieRepo.Setup(r => r.Update(1, It.IsAny<Movie>())).ReturnsAsync(MakeMovie());

        var result = await _sut.UpdateReview(1, new ReviewRequestDto { UserId = 1, MovieId = 1, Rating = 9, Comment = "Great" });
        Assert.Equal(9, result.Rating);
    }

    [Fact]
    public async Task UpdateReview_InvalidRating_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateReview(1, new ReviewRequestDto { Rating = 15 }));
    }

    [Fact]
    public async Task UpdateReview_NotFound_ThrowsNotFound()
    {
        _reviewRepo.Setup(r => r.Get(99)).ReturnsAsync((Review?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateReview(99, new ReviewRequestDto { Rating = 5 }));
    }

    [Fact]
    public async Task UpdateReview_UpdateReturnsNull_ThrowsException()
    {
        _reviewRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeReview());
        _reviewRepo.Setup(r => r.Update(1, It.IsAny<Review>())).ReturnsAsync((Review?)null);
        await Assert.ThrowsAsync<Exception>(() =>
            _sut.UpdateReview(1, new ReviewRequestDto { Rating = 5 }));
    }

    // ── DeleteReview ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteReview_Exists_ReturnsTrue()
    {
        var review = MakeReview();
        _reviewRepo.Setup(r => r.Get(1)).ReturnsAsync(review);
        _reviewRepo.Setup(r => r.Delete(1)).ReturnsAsync(review);
        _reviewRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Review>());
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _movieRepo.Setup(r => r.Update(1, It.IsAny<Movie>())).ReturnsAsync(MakeMovie());

        Assert.True(await _sut.DeleteReview(1));
    }

    [Fact]
    public async Task DeleteReview_NotFound_ThrowsNotFound()
    {
        _reviewRepo.Setup(r => r.Get(99)).ReturnsAsync((Review?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteReview(99));
    }

    [Fact]
    public async Task DeleteReview_DeleteReturnsNull_ThrowsException()
    {
        _reviewRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeReview());
        _reviewRepo.Setup(r => r.Delete(1)).ReturnsAsync((Review?)null);
        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteReview(1));
    }
}
