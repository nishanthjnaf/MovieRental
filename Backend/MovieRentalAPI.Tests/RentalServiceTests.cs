using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Models.Enums;
using MovieRentalAPI.Services;
using Xunit;

namespace MovieRentalAPI.Tests;

public class RentalServiceTests
{
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _itemRepo = new();
    private readonly Mock<IRepository<int, Inventory>> _invRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly Mock<IRepository<int, User>> _userRepo = new();
    private readonly Mock<IActivityLogService> _logService = new();
    private readonly RentalService _sut;

    public RentalServiceTests()
    {
        _logService.Setup(l => l.Log(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _sut = new RentalService(_rentalRepo.Object, _itemRepo.Object, _invRepo.Object,
            _movieRepo.Object, _userRepo.Object, _logService.Object);
    }

    private static User MakeUser(int id = 1) => new() { Id = id, Username = "u1" };
    private static Movie MakeMovie(int id = 1) => new() { Id = id, Title = "Film", RentalCount = 0, Genres = new List<Genre>() };
    private static Inventory MakeInv(int movieId = 1) =>
        new() { Id = 1, MovieId = movieId, RentalPrice = 10, IsAvailable = true };
    private static Rental MakeRental(int id = 1, RentalStatus status = RentalStatus.Available) =>
        new() { Id = id, UserId = 1, Status = status, TotalAmount = 30 };

    // ── CreateRental ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRental_Valid_ReturnsDto()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.Add(It.IsAny<Rental>()))
            .ReturnsAsync((Rental r) => { r.Id = 1; return r; });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>()))
            .ReturnsAsync((int _, Rental r) => r);
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        _invRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Inventory> { MakeInv() });
        _invRepo.Setup(r => r.Update(1, It.IsAny<Inventory>())).ReturnsAsync(MakeInv());
        _itemRepo.Setup(r => r.Add(It.IsAny<RentalItem>()))
            .ReturnsAsync((RentalItem i) => { i.Id = 1; return i; });
        _movieRepo.Setup(r => r.Update(1, It.IsAny<Movie>())).ReturnsAsync(MakeMovie());

        var result = await _sut.CreateRental(new CreateRentalRequestDto
        { UserId = 1, MovieIds = new List<int> { 1 }, RentalDays = 3 });

        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task CreateRental_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateRental(new CreateRentalRequestDto { UserId = 99, MovieIds = new List<int> { 1 } }));
    }

    [Fact]
    public async Task CreateRental_NoMovies_ThrowsBadRequest()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateRental(new CreateRentalRequestDto { UserId = 1, MovieIds = new List<int>() }));
    }

    [Fact]
    public async Task CreateRental_NullMovies_ThrowsBadRequest()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateRental(new CreateRentalRequestDto { UserId = 1, MovieIds = null! }));
    }

    [Fact]
    public async Task CreateRental_MovieNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.Add(It.IsAny<Rental>()))
            .ReturnsAsync((Rental r) => { r.Id = 1; return r; });
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        _movieRepo.Setup(r => r.Get(99)).ReturnsAsync((Movie?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateRental(new CreateRentalRequestDto { UserId = 1, MovieIds = new List<int> { 99 }, RentalDays = 3 }));
    }

    [Fact]
    public async Task CreateRental_MovieUnavailable_ThrowsConflict()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.Add(It.IsAny<Rental>()))
            .ReturnsAsync((Rental r) => { r.Id = 1; return r; });
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        _invRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Inventory>());

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateRental(new CreateRentalRequestDto { UserId = 1, MovieIds = new List<int> { 1 }, RentalDays = 3 }));
    }

    [Fact]
    public async Task CreateRental_AlreadyRented_ThrowsConflict()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.Add(It.IsAny<Rental>()))
            .ReturnsAsync((Rental r) => { r.Id = 1; return r; });
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _rentalRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Rental> { MakeRental() });
        _itemRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<RentalItem> { new() { Id = 1, RentalId = 1, MovieId = 1, IsActive = true } });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateRental(new CreateRentalRequestDto { UserId = 1, MovieIds = new List<int> { 1 }, RentalDays = 3 }));
    }

    // ── GetAllRentals ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllRentals_HasRentals_ReturnsAll()
    {
        _rentalRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Rental> { MakeRental(), MakeRental(2) });

        var result = (await _sut.GetAllRentals()).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllRentals_Empty_ThrowsNotFound()
    {
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllRentals());
    }

    // ── GetRentalsByUser ────────────────────────────────────────────────────

    [Fact]
    public async Task GetRentalsByUser_HasRentals_ReturnsAll()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Rental> { MakeRental() });

        var result = (await _sut.GetRentalsByUser(1)).ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetRentalsByUser_NoRentals_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetRentalsByUser(1));
    }

    // ── EndRentalItem ───────────────────────────────────────────────────────

    [Fact]
    public async Task EndRentalItem_Active_ReturnsTrue()
    {
        var item = new RentalItem { Id = 1, RentalId = 1, MovieId = 1, IsActive = true };
        _itemRepo.Setup(r => r.Get(1)).ReturnsAsync(item);
        _itemRepo.Setup(r => r.Update(1, It.IsAny<RentalItem>())).ReturnsAsync(item);

        Assert.True(await _sut.EndRentalItem(1));
        Assert.False(item.IsActive);
    }

    [Fact]
    public async Task EndRentalItem_NotFound_ThrowsNotFound()
    {
        _itemRepo.Setup(r => r.Get(99)).ReturnsAsync((RentalItem?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.EndRentalItem(99));
    }

    [Fact]
    public async Task EndRentalItem_AlreadyEnded_ThrowsConflict()
    {
        _itemRepo.Setup(r => r.Get(1)).ReturnsAsync(new RentalItem { Id = 1, IsActive = false });
        await Assert.ThrowsAsync<ConflictException>(() => _sut.EndRentalItem(1));
    }

    // ── RenewRentalItem ─────────────────────────────────────────────────────

    [Fact]
    public async Task RenewRentalItem_Valid_ExtendsDays()
    {
        var item = new RentalItem { Id = 1, RentalId = 1, MovieId = 1, IsActive = true, EndDate = DateTime.UtcNow.AddDays(5) };
        _itemRepo.Setup(r => r.Get(1)).ReturnsAsync(item);
        _itemRepo.Setup(r => r.Update(1, It.IsAny<RentalItem>())).ReturnsAsync(item);

        var result = await _sut.RenewRentalItem(1, new RenewRentalRequestDto { DaysToAdd = 3 });
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task RenewRentalItem_NullRequest_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.RenewRentalItem(1, null!));
    }

    [Fact]
    public async Task RenewRentalItem_ZeroDays_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.RenewRentalItem(1, new RenewRentalRequestDto { DaysToAdd = 0 }));
    }

    [Fact]
    public async Task RenewRentalItem_NotFound_ThrowsNotFound()
    {
        _itemRepo.Setup(r => r.Get(99)).ReturnsAsync((RentalItem?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.RenewRentalItem(99, new RenewRentalRequestDto { DaysToAdd = 3 }));
    }

    [Fact]
    public async Task RenewRentalItem_Expired_RenewsFromNow()
    {
        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = false,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1)
        };
        _itemRepo.Setup(r => r.Get(1)).ReturnsAsync(item);
        _itemRepo.Setup(r => r.Update(1, It.IsAny<RentalItem>())).ReturnsAsync(item);

        var result = await _sut.RenewRentalItem(1, new RenewRentalRequestDto { DaysToAdd = 5 });
        Assert.True(result.IsActive);
        Assert.True(result.EndDate > DateTime.UtcNow);
    }
}

// ── Additional coverage tests ────────────────────────────────────────────────

public class RentalServiceCoverageTests
{
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _itemRepo = new();
    private readonly Mock<IRepository<int, Inventory>> _invRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly Mock<IRepository<int, User>> _userRepo = new();
    private readonly Mock<IActivityLogService> _logService = new();
    private readonly RentalService _sut;

    public RentalServiceCoverageTests()
    {
        _logService.Setup(l => l.Log(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _sut = new RentalService(_rentalRepo.Object, _itemRepo.Object, _invRepo.Object,
            _movieRepo.Object, _userRepo.Object, _logService.Object);
    }

    private static User MakeUser(int id = 1) => new() { Id = id, Username = "u1" };
    private static Movie MakeMovie(int id = 1) => new() { Id = id, Title = "Film", RentalCount = 0, Genres = new List<Genre>() };
    private static Inventory MakeInv(int movieId = 1) => new() { Id = 1, MovieId = movieId, RentalPrice = 10, IsAvailable = true };
    private static Rental MakeRental(int id = 1, RentalStatus status = RentalStatus.Available, int userId = 1) =>
        new() { Id = id, UserId = userId, Status = status, TotalAmount = 30, RentalDate = DateTime.UtcNow };

    // ── GetActiveRentals ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveRentals_HasActiveItems_ReturnsItems()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental> { MakeRental() });
        var item = new RentalItem { Id = 1, RentalId = 1, MovieId = 1, IsActive = true, EndDate = DateTime.UtcNow.AddDays(5) };
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem> { item });

        var result = (await _sut.GetActiveRentals(1)).ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetActiveRentals_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetActiveRentals(99));
    }

    [Fact]
    public async Task GetActiveRentals_NoItems_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental> { MakeRental() });
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetActiveRentals(1));
    }

    [Fact]
    public async Task GetActiveRentals_DeactivatesExpiredItems()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental> { MakeRental() });
        var expiredItem = new RentalItem { Id = 1, RentalId = 1, MovieId = 1, IsActive = true, EndDate = DateTime.UtcNow.AddDays(-1) };
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem> { expiredItem });
        _itemRepo.Setup(r => r.Update(1, It.IsAny<RentalItem>())).ReturnsAsync(expiredItem);

        // Call GetActiveRentals — it deactivates expired items then returns them (even if inactive)
        await _sut.GetActiveRentals(1);

        _itemRepo.Verify(r => r.Update(1, It.Is<RentalItem>(i => !i.IsActive)), Times.Once);
    }

    [Fact]
    public async Task GetActiveRentals_NoActiveRentals_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        // Only PaymentPending rentals — no Available ones
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>
            { MakeRental(1, RentalStatus.PaymentPending) });
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetActiveRentals(1));
    }

    // ── GetRentalItemsByRentalId ──────────────────────────────────────────────

    [Fact]
    public async Task GetRentalItemsByRentalId_HasItems_ReturnsAll()
    {
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeRental());
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>
        {
            new() { Id = 1, RentalId = 1, MovieId = 1, PricePerDay = 10 },
            new() { Id = 2, RentalId = 1, MovieId = 2, PricePerDay = 15 }
        });

        var result = (await _sut.GetRentalItemsByRentalId(1)).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRentalItemsByRentalId_RentalNotFound_ThrowsNotFound()
    {
        _rentalRepo.Setup(r => r.Get(99)).ReturnsAsync((Rental?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetRentalItemsByRentalId(99));
    }

    [Fact]
    public async Task GetRentalItemsByRentalId_NoItems_ThrowsNotFound()
    {
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeRental());
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetRentalItemsByRentalId(1));
    }

    // ── GetAllRentals — expired pending path ──────────────────────────────────

    [Fact]
    public async Task GetAllRentals_ExpiredPendingRental_UpdatesStatusToNotDone()
    {
        var oldRental = new Rental
        {
            Id = 1, UserId = 1, Status = RentalStatus.PaymentPending,
            RentalDate = DateTime.UtcNow.AddMinutes(-30), // older than 20 min cutoff
            TotalAmount = 50
        };
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental> { oldRental });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>())).ReturnsAsync(oldRental);

        var result = (await _sut.GetAllRentals()).ToList();
        Assert.Single(result);
        _rentalRepo.Verify(r => r.Update(1, It.IsAny<Rental>()), Times.Once);
    }

    // ── CreateRental — RentalDaysPerMovie path ────────────────────────────────

    [Fact]
    public async Task CreateRental_WithRentalDaysPerMovie_UsesPerMovieDays()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.Add(It.IsAny<Rental>()))
            .ReturnsAsync((Rental r) => { r.Id = 1; return r; });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>()))
            .ReturnsAsync((int _, Rental r) => r);
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        _invRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Inventory> { MakeInv() });
        _invRepo.Setup(r => r.Update(1, It.IsAny<Inventory>())).ReturnsAsync(MakeInv());
        _itemRepo.Setup(r => r.Add(It.IsAny<RentalItem>()))
            .ReturnsAsync((RentalItem i) => { i.Id = 1; return i; });
        _movieRepo.Setup(r => r.Update(1, It.IsAny<Movie>())).ReturnsAsync(MakeMovie());

        var result = await _sut.CreateRental(new CreateRentalRequestDto
        {
            UserId = 1,
            MovieIds = new List<int> { 1 },
            RentalDays = 5,
            RentalDaysPerMovie = new List<int> { 7 } // per-movie override
        });

        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task CreateRental_DaysLessThan3_ClampsTo3()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.Add(It.IsAny<Rental>()))
            .ReturnsAsync((Rental r) => { r.Id = 1; return r; });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>()))
            .ReturnsAsync((int _, Rental r) => r);
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        _invRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Inventory> { MakeInv() });
        _invRepo.Setup(r => r.Update(1, It.IsAny<Inventory>())).ReturnsAsync(MakeInv());
        _itemRepo.Setup(r => r.Add(It.IsAny<RentalItem>()))
            .ReturnsAsync((RentalItem i) => { i.Id = 1; return i; });
        _movieRepo.Setup(r => r.Update(1, It.IsAny<Movie>())).ReturnsAsync(MakeMovie());

        // Days = 1 should be clamped to 3, total = 10 * 3 = 30
        var result = await _sut.CreateRental(new CreateRentalRequestDto
        { UserId = 1, MovieIds = new List<int> { 1 }, RentalDays = 1 });

        Assert.Equal(30, result.TotalAmount);
    }

    [Fact]
    public async Task CreateRental_ZeroDays_ClampsTo3()
    {
        _userRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeUser());
        _rentalRepo.Setup(r => r.Add(It.IsAny<Rental>()))
            .ReturnsAsync((Rental r) => { r.Id = 1; return r; });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>()))
            .ReturnsAsync((int _, Rental r) => r);
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        _rentalRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Rental>());
        _invRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Inventory> { MakeInv() });
        _invRepo.Setup(r => r.Update(1, It.IsAny<Inventory>())).ReturnsAsync(MakeInv());
        _itemRepo.Setup(r => r.Add(It.IsAny<RentalItem>()))
            .ReturnsAsync((RentalItem i) => { i.Id = 1; return i; });
        _movieRepo.Setup(r => r.Update(1, It.IsAny<Movie>())).ReturnsAsync(MakeMovie());

        var result = await _sut.CreateRental(new CreateRentalRequestDto
        { UserId = 1, MovieIds = new List<int> { 1 }, RentalDays = 0 });

        Assert.Equal(30, result.TotalAmount);
    }

    // ── GetRentalsByUser — user not found ─────────────────────────────────────

    [Fact]
    public async Task GetRentalsByUser_UserNotFound_ThrowsNotFound()
    {
        _userRepo.Setup(r => r.Get(99)).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetRentalsByUser(99));
    }
}
