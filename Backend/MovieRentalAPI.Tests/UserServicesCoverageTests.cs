using Microsoft.EntityFrameworkCore;
using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;
using MovieRentalModels;
using Xunit;

namespace MovieRentalAPI.Tests;

/// <summary>Extra tests targeting remaining uncovered branches in UserServices.</summary>
public class UserServicesCoverageTests
{
    private readonly MovieRentalContext _ctx;
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _rentalItemRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly Mock<IRepository<int, User>> _userRepo = new();
    private readonly Mock<IActivityLogService> _logService = new();
    private readonly UserServices _sut;

    public UserServicesCoverageTests()
    {
        var opts = new DbContextOptionsBuilder<MovieRentalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _ctx = new MovieRentalContext(opts);
        _logService.Setup(l => l.Log(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var tokenSvc = new Mock<ITokenService>();
        tokenSvc.Setup(t => t.CreateToken(It.IsAny<TokenPayloadDto>())).Returns("tok");
        _sut = new UserServices(_ctx, new PasswordService(), tokenSvc.Object,
            _rentalRepo.Object, _movieRepo.Object, _userRepo.Object,
            _rentalItemRepo.Object, new NotificationService(_ctx), _logService.Object);
    }

    // ── GetAllRentedMovies — rental with no items ────────────────────────────

    [Fact]
    public async Task GetAllRentedMovies_RentalWithNoItems_ReturnsEmpty()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(new User { Id = 1, Username = "u" });
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>
            { new() { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.Available } });
        _rentalItemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>()); // no items

        var result = (await _sut.GetAllRentedMovies(1)).ToList();
        Assert.Empty(result);
    }

    // ── GetAllRentedMovies — movie is null ───────────────────────────────────

    [Fact]
    public async Task GetAllRentedMovies_MovieNotFound_UsesEmptyTitle()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(new User { Id = 1, Username = "u" });
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>
            { new() { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.Available } });
        _rentalItemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>
        {
            new() { Id = 1, RentalId = 1, MovieId = 99, IsActive = true,
                    PricePerDay = 10, StartDate = DateTime.UtcNow.AddDays(-3), EndDate = DateTime.UtcNow.AddDays(4) }
        });
        _movieRepo.Setup(r => r.Get(99)).ReturnsAsync((Movie?)null);

        var result = (await _sut.GetAllRentedMovies(1)).ToList();
        Assert.Single(result);
        Assert.Equal("", result[0].MovieTitle);
        Assert.Null(result[0].PosterPath);
    }

    // ── ResetPassword — update returns null ──────────────────────────────────

    [Fact]
    public async Task ResetPassword_UpdateReturnsNull_ThrowsException()
    {
        var hash = new PasswordService().HashPassword("oldpass", null, out var key);
        var user = new User { Id = 1, Username = "u", Password = hash, PasswordHash = key! };
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(user);
        _userRepo.Setup(r => r.Update(1, It.IsAny<User>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _sut.ResetPassword(1, new ResetPasswordRequestDto
            { OldPassword = "oldpass", NewPassword = "newpass", ConfirmPassword = "newpass" }));
    }

    // ── SavePreferences — null lists ─────────────────────────────────────────

    [Fact]
    public async Task SavePreferences_NullLists_SavesEmpty()
    {
        var result = await _sut.SavePreferences(1, new SavePreferenceRequestDto
        { PreferredGenres = null, PreferredLanguages = null, Theme = null });

        Assert.True(result.IsSet);
        Assert.Empty(result.PreferredGenres);
        Assert.Empty(result.PreferredLanguages);
        Assert.Equal("dark", result.Theme);
    }

    // ── GetAllUsers — list is empty ──────────────────────────────────────────

    [Fact]
    public async Task GetAllUsers_EmptyList_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<User>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllUsers());
    }
}
