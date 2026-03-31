using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;
using Xunit;

namespace MovieRentalAPI.Tests;

public class WatchlistServiceTests
{
    private readonly Mock<IRepository<int, Watchlist>> _wlRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly Mock<IRepository<int, User>> _userRepo = new();
    private readonly WatchlistService _sut;

    public WatchlistServiceTests()
    {
        _sut = new WatchlistService(_wlRepo.Object, _movieRepo.Object, _userRepo.Object);
    }

    private static User MakeUser(int id = 1) => new() { Id = id, Username = "user1" };
    private static Movie MakeMovie(int id = 1) => new() { Id = id, Title = "Film", Genres = new List<Genre>() };
    private static Watchlist MakeWl(int id = 1, int userId = 1, int movieId = 1) =>
        new() { Id = id, UserId = userId, MovieId = movieId };

    // ── AddToWatchlist ──────────────────────────────────────────────────────

    [Fact]
    public async Task AddToWatchlist_Valid_ReturnsDto()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _wlRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Watchlist>());
        _wlRepo.Setup(r => r.Add(It.IsAny<Watchlist>()))
            .ReturnsAsync((Watchlist w) => { w.Id = 1; return w; });

        var result = await _sut.AddToWatchlist(new WatchlistRequestDto { UserId = 1, MovieId = 1 });
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.MovieId);
        Assert.Equal("Film", result.MovieTitle);
    }

    [Fact]
    public async Task AddToWatchlist_InvalidUserId_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddToWatchlist(new WatchlistRequestDto { UserId = 0, MovieId = 1 }));
    }

    [Fact]
    public async Task AddToWatchlist_InvalidMovieId_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddToWatchlist(new WatchlistRequestDto { UserId = 1, MovieId = 0 }));
    }

    [Fact]
    public async Task AddToWatchlist_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AddToWatchlist(new WatchlistRequestDto { UserId = 99, MovieId = 1 }));
    }

    [Fact]
    public async Task AddToWatchlist_MovieNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(99)).ReturnsAsync((Movie?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AddToWatchlist(new WatchlistRequestDto { UserId = 1, MovieId = 99 }));
    }

    [Fact]
    public async Task AddToWatchlist_AlreadyExists_ThrowsConflict()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _wlRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Watchlist> { MakeWl() });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.AddToWatchlist(new WatchlistRequestDto { UserId = 1, MovieId = 1 }));
    }

    [Fact]
    public async Task AddToWatchlist_AddReturnsNull_ThrowsException()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _wlRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Watchlist>());
        _wlRepo.Setup(r => r.Add(It.IsAny<Watchlist>())).ReturnsAsync((Watchlist?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _sut.AddToWatchlist(new WatchlistRequestDto { UserId = 1, MovieId = 1 }));
    }

    // ── GetUserWatchlist ────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserWatchlist_HasItems_ReturnsAll()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _wlRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Watchlist> { MakeWl(), MakeWl(2) });
        _movieRepo.Setup(r => r.Get(It.IsAny<int>())).ReturnsAsync(MakeMovie());

        var result = (await _sut.GetUserWatchlist(1)).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetUserWatchlist_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserWatchlist(99));
    }

    [Fact]
    public async Task GetUserWatchlist_Empty_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _wlRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Watchlist>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserWatchlist(1));
    }

    [Fact]
    public async Task GetUserWatchlist_MovieNotFound_ReturnsEmptyTitle()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _wlRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Watchlist> { MakeWl() });
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync((Movie?)null);

        var result = (await _sut.GetUserWatchlist(1)).ToList();
        Assert.Equal("", result[0].MovieTitle);
    }

    // ── RemoveFromWatchlist ─────────────────────────────────────────────────

    [Fact]
    public async Task RemoveFromWatchlist_Exists_ReturnsTrue()
    {
        var wl = MakeWl();
        _wlRepo.Setup(r => r.Get(1)).ReturnsAsync(wl);
        _wlRepo.Setup(r => r.Delete(1)).ReturnsAsync(wl);
        Assert.True(await _sut.RemoveFromWatchlist(1));
    }

    [Fact]
    public async Task RemoveFromWatchlist_NotFound_ThrowsNotFound()
    {
        _wlRepo.Setup(r => r.Get(99)).ReturnsAsync((Watchlist?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoveFromWatchlist(99));
    }

    [Fact]
    public async Task RemoveFromWatchlist_DeleteReturnsNull_ThrowsException()
    {
        _wlRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeWl());
        _wlRepo.Setup(r => r.Delete(1)).ReturnsAsync((Watchlist?)null);
        await Assert.ThrowsAsync<Exception>(() => _sut.RemoveFromWatchlist(1));
    }
}
