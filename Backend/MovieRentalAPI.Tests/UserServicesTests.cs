using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Repositories;
using MovieRentalAPI.Services;
using MovieRentalModels;
using Xunit;

namespace MovieRentalAPI.Tests;

public class UserServicesTests
{
    private readonly MovieRentalContext _ctx;
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _rentalItemRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly Mock<IRepository<int, User>> _userRepo = new();
    private readonly Mock<IActivityLogService> _logService = new();
    private readonly PasswordService _passwordService = new();
    private readonly UserServices _sut;

    public UserServicesTests()
    {
        var opts = new DbContextOptionsBuilder<MovieRentalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _ctx = new MovieRentalContext(opts);

        _logService.Setup(l => l.Log(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _tokenService.Setup(t => t.CreateToken(It.IsAny<TokenPayloadDto>())).Returns("mock_token");

        var notifService = new NotificationService(_ctx);

        _sut = new UserServices(_ctx, _passwordService, _tokenService.Object,
            _rentalRepo.Object, _movieRepo.Object, _userRepo.Object,
            _rentalItemRepo.Object, notifService, _logService.Object);
    }

    private User SeedUser(string username = "alice", string role = "Customer")
    {
        var hash = _passwordService.HashPassword("password123", null, out var key);
        var user = new User
        {
            Id = 0, Username = username, Password = hash, PasswordHash = key!,
            Role = role, Name = "Alice", Email = "alice@test.com", Phone = "1234567890"
        };
        _ctx.Users.Add(user);
        _ctx.SaveChanges();
        return user;
    }

    // ── CheckUser ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckUser_ValidCredentials_ReturnsToken()
    {
        SeedUser();
        var result = await _sut.CheckUser(new CheckUserRequestDto { Username = "alice", Password = "password123" });
        Assert.Equal("mock_token", result.Token);
        Assert.Equal("alice", result.Username);
    }

    [Fact]
    public async Task CheckUser_EmptyUsername_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CheckUser(new CheckUserRequestDto { Username = "", Password = "pass" }));
    }

    [Fact]
    public async Task CheckUser_EmptyPassword_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CheckUser(new CheckUserRequestDto { Username = "alice", Password = "" }));
    }

    [Fact]
    public async Task CheckUser_WrongUsername_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnAuthorizedException>(() =>
            _sut.CheckUser(new CheckUserRequestDto { Username = "nobody", Password = "pass" }));
    }

    [Fact]
    public async Task CheckUser_WrongPassword_ThrowsUnauthorized()
    {
        SeedUser();
        await Assert.ThrowsAsync<UnAuthorizedException>(() =>
            _sut.CheckUser(new CheckUserRequestDto { Username = "alice", Password = "wrongpass" }));
    }

    // ── RegisterUser ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterUser_Valid_ReturnsSuccessMessage()
    {
        var result = await _sut.RegisterUser(new RegisterUserRequestDto
        { Username = "newuser", Password = "pass123", Email = "new@test.com", Name = "New" });

        Assert.Equal("newuser", result.Username);
        Assert.Contains("successfully", result.Message);
    }

    [Fact]
    public async Task RegisterUser_EmptyUsername_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.RegisterUser(new RegisterUserRequestDto { Username = "", Password = "pass", Email = "e@e.com" }));
    }

    [Fact]
    public async Task RegisterUser_EmptyPassword_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.RegisterUser(new RegisterUserRequestDto { Username = "u", Password = "", Email = "e@e.com" }));
    }

    [Fact]
    public async Task RegisterUser_EmptyEmail_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.RegisterUser(new RegisterUserRequestDto { Username = "u", Password = "p", Email = "" }));
    }

    [Fact]
    public async Task RegisterUser_DuplicateUsername_ThrowsConflict()
    {
        SeedUser("dupuser");
        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.RegisterUser(new RegisterUserRequestDto
            { Username = "dupuser", Password = "pass", Email = "e@e.com" }));
    }

    // ── GetUserById ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserById_Exists_ReturnsDto()
    {
        var user = SeedUser();
        _userRepo.Setup(r => r.Get(user.Id)).ReturnsAsync(user);
        var result = await _sut.GetUserById(user.Id);
        Assert.Equal("alice", result.Username);
    }

    [Fact]
    public async Task GetUserById_NotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserById(99));
    }

    // ── GetUserByUsername ───────────────────────────────────────────────────

    [Fact]
    public async Task GetUserByUsername_Exists_ReturnsDto()
    {
        SeedUser("charlie");
        var result = await _sut.GetUserByUsername("charlie");
        Assert.Equal("charlie", result.Username);
    }

    [Fact]
    public async Task GetUserByUsername_Empty_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.GetUserByUsername(""));
    }

    [Fact]
    public async Task GetUserByUsername_NotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserByUsername("nobody"));
    }

    // ── GetAllUsers ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsers_HasUsers_ReturnsAll()
    {
        _userRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<User> { new() { Id = 1, Username = "u1" } });
        var result = (await _sut.GetAllUsers()).ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllUsers_Empty_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.GetAll()).ReturnsAsync((IEnumerable<User>?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllUsers());
    }

    // ── UpdateUser ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUser_Valid_ReturnsUpdated()
    {
        var user = new User { Id = 1, Username = "old", Role = "Customer" };
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(user);
        _userRepo.Setup(r => r.Update(1, It.IsAny<User>())).ReturnsAsync(user);

        var result = await _sut.UpdateUser(1, new UpdateUserRequestDto
        { Username = "new", Role = "Customer", Name = "N", Email = "e@e.com", Phone = "0" });

        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateUser_NotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateUser(99, new UpdateUserRequestDto { Username = "x" }));
    }

    [Fact]
    public async Task UpdateUser_EmptyUsername_ThrowsBadRequest()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(new User { Id = 1, Username = "u" });
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateUser(1, new UpdateUserRequestDto { Username = "" }));
    }

    [Fact]
    public async Task UpdateUser_UpdateReturnsNull_ThrowsException()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(new User { Id = 1, Username = "u" });
        _userRepo.Setup(r => r.Update(1, It.IsAny<User>())).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<Exception>(() =>
            _sut.UpdateUser(1, new UpdateUserRequestDto { Username = "x" }));
    }

    // ── DeleteUser ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUser_Exists_ReturnsTrue()
    {
        var user = new User { Id = 1, Username = "u" };
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(user);
        _userRepo.Setup(r => r.Delete(1)).ReturnsAsync(user);
        Assert.True(await _sut.DeleteUser(1));
    }

    [Fact]
    public async Task DeleteUser_NotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteUser(99));
    }

    [Fact]
    public async Task DeleteUser_DeleteReturnsNull_ThrowsException()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(new User { Id = 1, Username = "u" });
        _userRepo.Setup(r => r.Delete(1)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteUser(1));
    }

    // ── ResetPassword ───────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_Valid_ReturnsTrue()
    {
        var user = SeedUser();
        _userRepo.Setup(r => r.Get(user.Id)).ReturnsAsync(user);
        _userRepo.Setup(r => r.Update(user.Id, It.IsAny<User>())).ReturnsAsync(user);

        var result = await _sut.ResetPassword(user.Id, new ResetPasswordRequestDto
        { OldPassword = "password123", NewPassword = "newpass456", ConfirmPassword = "newpass456" });

        Assert.True(result);
    }

    [Fact]
    public async Task ResetPassword_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ResetPassword(99, new ResetPasswordRequestDto
            { OldPassword = "a", NewPassword = "b", ConfirmPassword = "b" }));
    }

    [Fact]
    public async Task ResetPassword_EmptyFields_ThrowsBadRequest()
    {
        var user = SeedUser();
        _userRepo.Setup(r => r.Get(user.Id)).ReturnsAsync(user);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.ResetPassword(user.Id, new ResetPasswordRequestDto
            { OldPassword = "", NewPassword = "b", ConfirmPassword = "b" }));
    }

    [Fact]
    public async Task ResetPassword_PasswordMismatch_ThrowsBadRequest()
    {
        var user = SeedUser();
        _userRepo.Setup(r => r.Get(user.Id)).ReturnsAsync(user);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.ResetPassword(user.Id, new ResetPasswordRequestDto
            { OldPassword = "password123", NewPassword = "new1", ConfirmPassword = "new2" }));
    }

    [Fact]
    public async Task ResetPassword_WrongOldPassword_ThrowsBadRequest()
    {
        var user = SeedUser();
        _userRepo.Setup(r => r.Get(user.Id)).ReturnsAsync(user);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.ResetPassword(user.Id, new ResetPasswordRequestDto
            { OldPassword = "wrongold", NewPassword = "new1", ConfirmPassword = "new1" }));
    }

    // ── SavePreferences / GetPreferences ────────────────────────────────────

    [Fact]
    public async Task SavePreferences_NewUser_CreatesPreference()
    {
        var result = await _sut.SavePreferences(1, new SavePreferenceRequestDto
        { PreferredGenres = new List<string> { "Action" }, PreferredLanguages = new List<string> { "English" }, Theme = "dark" });

        Assert.True(result.IsSet);
        Assert.Contains("Action", result.PreferredGenres);
    }

    [Fact]
    public async Task SavePreferences_ExistingUser_UpdatesPreference()
    {
        await _sut.SavePreferences(1, new SavePreferenceRequestDto
        { PreferredGenres = new List<string> { "Drama" }, PreferredLanguages = new List<string>(), Theme = "light" });

        var result = await _sut.SavePreferences(1, new SavePreferenceRequestDto
        { PreferredGenres = new List<string> { "Comedy" }, PreferredLanguages = new List<string>(), Theme = "dark" });

        Assert.Contains("Comedy", result.PreferredGenres);
    }

    [Fact]
    public async Task GetPreferences_NoPreference_ReturnsNotSet()
    {
        var result = await _sut.GetPreferences(999);
        Assert.False(result.IsSet);
    }

    [Fact]
    public async Task GetPreferences_HasPreference_ReturnsDto()
    {
        await _sut.SavePreferences(5, new SavePreferenceRequestDto
        { PreferredGenres = new List<string> { "Sci-Fi" }, PreferredLanguages = new List<string> { "Tamil" }, Theme = "dark" });

        var result = await _sut.GetPreferences(5);
        Assert.True(result.IsSet);
        Assert.Contains("Sci-Fi", result.PreferredGenres);
    }

    // ── GetAllRentedMovies ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAllRentedMovies_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllRentedMovies(99));
    }

    [Fact]
    public async Task GetAllRentedMovies_NoRentals_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(new User { Id = 1, Username = "u" });
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllRentedMovies(1));
    }
}

public class UserServicesRentedMoviesTests
{
    private readonly MovieRentalContext _ctx;
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _rentalItemRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly Mock<IRepository<int, User>> _userRepo = new();
    private readonly Mock<IActivityLogService> _logService = new();
    private readonly UserServices _sut;

    public UserServicesRentedMoviesTests()
    {
        var opts = new DbContextOptionsBuilder<MovieRentalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _ctx = new MovieRentalContext(opts);

        _logService.Setup(l => l.Log(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var tokenSvc = new Mock<ITokenService>();
        var notif = new NotificationService(_ctx);

        _sut = new UserServices(_ctx, new PasswordService(), tokenSvc.Object,
            _rentalRepo.Object, _movieRepo.Object, _userRepo.Object,
            _rentalItemRepo.Object, notif, _logService.Object);
    }

    [Fact]
    public async Task GetAllRentedMovies_ActiveRental_ReturnsMovies()
    {
        var user = new User { Id = 1, Username = "u1" };
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(user);

        var rental = new Rental { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.Available };
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental> { rental });

        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = true,
            PricePerDay = 10,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(4)
        };
        _rentalItemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem> { item });
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(
            new Movie { Id = 1, Title = "Film", Language = "En", Genres = new List<Genre>() });

        var result = (await _sut.GetAllRentedMovies(1)).ToList();
        Assert.Single(result);
        Assert.Equal("Film", result[0].MovieTitle);
    }

    [Fact]
    public async Task GetAllRentedMovies_ExpiredItem_DeactivatesIt()
    {
        var user = new User { Id = 1, Username = "u1" };
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(user);

        var rental = new Rental { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.Available };
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental> { rental });

        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = true,
            PricePerDay = 10,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(-1) // already expired
        };
        _rentalItemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem> { item });
        _rentalItemRepo.Setup(r => r.Update(1, It.IsAny<RentalItem>())).ReturnsAsync(item);
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(
            new Movie { Id = 1, Title = "Film", Language = "En", Genres = new List<Genre>() });

        await _sut.GetAllRentedMovies(1);

        _rentalItemRepo.Verify(r => r.Update(1, It.Is<RentalItem>(i => !i.IsActive)), Times.Once);
    }

    [Fact]
    public async Task GetAllRentedMovies_PendingRental_SkipsIt()
    {
        var user = new User { Id = 1, Username = "u1" };
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(user);

        // Rental is PaymentPending — should be skipped
        var rental = new Rental { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.PaymentPending };
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental> { rental });
        _rentalItemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());

        var result = (await _sut.GetAllRentedMovies(1)).ToList();
        Assert.Empty(result);
    }
}
