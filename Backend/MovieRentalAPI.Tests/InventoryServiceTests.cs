using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;
using Xunit;

namespace MovieRentalAPI.Tests;

public class InventoryServiceTests
{
    private readonly Mock<IRepository<int, Inventory>> _invRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _sut = new InventoryService(_invRepo.Object, _movieRepo.Object);
    }

    private static Movie MakeMovie(int id = 1) => new() { Id = id, Title = "Film", Genres = new List<Genre>() };
    private static Inventory MakeInv(int id = 1, int movieId = 1) =>
        new() { Id = id, MovieId = movieId, RentalPrice = 10, IsAvailable = true, Movie = MakeMovie(movieId) };

    // ── AddInventory ────────────────────────────────────────────────────────

    [Fact]
    public async Task AddInventory_Valid_ReturnsDto()
    {
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _invRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(new List<Inventory>());
        _invRepo.Setup(r => r.Add(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => { i.Id = 1; return i; });
        _invRepo.Setup(r => r.GetIncluding(1, It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(MakeInv());

        var result = await _sut.AddInventory(new InventoryRequestDto { MovieId = 1, RentalPrice = 10, IsAvailable = true });
        Assert.Equal(1, result.MovieId);
        Assert.Equal(10, result.RentalPrice);
    }

    [Fact]
    public async Task AddInventory_InvalidMovieId_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddInventory(new InventoryRequestDto { MovieId = 0, RentalPrice = 10 }));
    }

    [Fact]
    public async Task AddInventory_ZeroPrice_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddInventory(new InventoryRequestDto { MovieId = 1, RentalPrice = 0 }));
    }

    [Fact]
    public async Task AddInventory_NegativePrice_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddInventory(new InventoryRequestDto { MovieId = 1, RentalPrice = -5 }));
    }

    [Fact]
    public async Task AddInventory_MovieNotFound_ThrowsNotFound()
    {
        _movieRepo.Setup(r => r.Get(99)).ReturnsAsync((Movie?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AddInventory(new InventoryRequestDto { MovieId = 99, RentalPrice = 10 }));
    }

    [Fact]
    public async Task AddInventory_AlreadyExists_ThrowsConflict()
    {
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _invRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(new List<Inventory> { MakeInv() });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.AddInventory(new InventoryRequestDto { MovieId = 1, RentalPrice = 10 }));
    }

    // ── GetAllInventory ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllInventory_ReturnsAll()
    {
        _invRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(new List<Inventory> { MakeInv(1), MakeInv(2, 2) });

        var result = (await _sut.GetAllInventory()).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllInventory_Empty_ThrowsNotFound()
    {
        _invRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(new List<Inventory>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllInventory());
    }

    // ── GetInventoryById ────────────────────────────────────────────────────

    [Fact]
    public async Task GetInventoryById_Exists_ReturnsDto()
    {
        _invRepo.Setup(r => r.GetIncluding(1, It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(MakeInv());
        var result = await _sut.GetInventoryById(1);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetInventoryById_NotFound_ThrowsNotFound()
    {
        _invRepo.Setup(r => r.GetIncluding(99, It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync((Inventory?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetInventoryById(99));
    }

    // ── GetInventoryByMovie ─────────────────────────────────────────────────

    [Fact]
    public async Task GetInventoryByMovie_Exists_ReturnsDto()
    {
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _invRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(new List<Inventory> { MakeInv() });

        var result = await _sut.GetInventoryByMovie(1);
        Assert.Equal(1, result.MovieId);
    }

    [Fact]
    public async Task GetInventoryByMovie_MovieNotFound_ThrowsNotFound()
    {
        _movieRepo.Setup(r => r.Get(99)).ReturnsAsync((Movie?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetInventoryByMovie(99));
    }

    [Fact]
    public async Task GetInventoryByMovie_NoInventory_ThrowsNotFound()
    {
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeMovie());
        _invRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(new List<Inventory>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetInventoryByMovie(1));
    }

    // ── UpdateInventory ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateInventory_Valid_ReturnsUpdated()
    {
        _invRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeInv());
        _invRepo.Setup(r => r.Update(1, It.IsAny<Inventory>())).ReturnsAsync(MakeInv());
        _invRepo.Setup(r => r.GetIncluding(1, It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object>>[]>()))
            .ReturnsAsync(MakeInv());

        var result = await _sut.UpdateInventory(1, new InventoryRequestDto { RentalPrice = 20, IsAvailable = false });
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateInventory_ZeroPrice_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateInventory(1, new InventoryRequestDto { RentalPrice = 0 }));
    }

    [Fact]
    public async Task UpdateInventory_NotFound_ThrowsNotFound()
    {
        _invRepo.Setup(r => r.Get(99)).ReturnsAsync((Inventory?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateInventory(99, new InventoryRequestDto { RentalPrice = 10 }));
    }

    // ── DeleteInventory ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteInventory_Exists_ReturnsTrue()
    {
        var inv = MakeInv();
        _invRepo.Setup(r => r.Get(1)).ReturnsAsync(inv);
        _invRepo.Setup(r => r.Delete(1)).ReturnsAsync(inv);
        Assert.True(await _sut.DeleteInventory(1));
    }

    [Fact]
    public async Task DeleteInventory_NotFound_ThrowsNotFound()
    {
        _invRepo.Setup(r => r.Get(99)).ReturnsAsync((Inventory?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteInventory(99));
    }

    [Fact]
    public async Task DeleteInventory_DeleteReturnsNull_ThrowsException()
    {
        _invRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeInv());
        _invRepo.Setup(r => r.Delete(1)).ReturnsAsync((Inventory?)null);
        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteInventory(1));
    }

    // ── ToggleAvailability ──────────────────────────────────────────────────

    [Fact]
    public async Task ToggleAvailability_Available_BecomesUnavailable()
    {
        var inv = MakeInv();
        _invRepo.Setup(r => r.Get(1)).ReturnsAsync(inv);
        _invRepo.Setup(r => r.Update(1, It.IsAny<Inventory>())).ReturnsAsync(inv);

        var result = await _sut.ToggleAvailability(1);
        Assert.True(result);
        Assert.False(inv.IsAvailable);
    }

    [Fact]
    public async Task ToggleAvailability_NotFound_ThrowsNotFound()
    {
        _invRepo.Setup(r => r.Get(99)).ReturnsAsync((Inventory?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleAvailability(99));
    }
}
