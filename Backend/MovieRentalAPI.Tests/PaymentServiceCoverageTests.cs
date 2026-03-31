using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Models.Enums;
using MovieRentalAPI.Services;
using MovieRentalModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MovieRentalAPI.Tests;

/// <summary>Covers remaining branches in PaymentService.</summary>
public class PaymentServiceCoverageTests
{
    private readonly Mock<IRepository<int, Payment>> _payRepo = new();
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _itemRepo = new();
    private readonly Mock<IActivityLogService> _logService = new();
    private readonly MovieRentalContext _ctx;
    private readonly PaymentService _sut;

    public PaymentServiceCoverageTests()
    {
        var opts = new DbContextOptionsBuilder<MovieRentalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _ctx = new MovieRentalContext(opts);
        _logService.Setup(l => l.Log(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _sut = new PaymentService(_payRepo.Object, _rentalRepo.Object, _itemRepo.Object,
            _ctx, new NotificationService(_ctx), _logService.Object);
    }

    private static Payment MakePayment(int rentalId = 1, int userId = 1) => new()
    {
        Id = 1, RentalId = rentalId, UserId = userId, Amount = 100,
        PaymentMethod = PaymentMethod.CreditCard, PaymentType = PaymentType.Purchase,
        Status = PaymentStatus.Success, PaymentId = "PAY_X", PaymentDate = DateTime.UtcNow,
        User = new User { Id = userId, Username = "u1", Name = "User" }
    };

    // ── GetPaymentsByRental — null payments ──────────────────────────────────

    [Fact]
    public async Task GetPaymentsByRental_NullPayments_ThrowsNotFound()
    {
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(new Rental { Id = 1, UserId = 1 });
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync((IEnumerable<Payment>)null!);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetPaymentsByRental(1));
    }

    // ── GetPaymentByUser — null payments ─────────────────────────────────────

    [Fact]
    public async Task GetPaymentByUser_NullPayments_ThrowsNotFound()
    {
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync((IEnumerable<Payment>)null!);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetPaymentByUser(1));
    }

    // ── GetAllPayments — null ────────────────────────────────────────────────

    [Fact]
    public async Task GetAllPayments_NullPayments_ThrowsNotFound()
    {
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync((IEnumerable<Payment>)null!);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllPayments());
    }

    // ── MakePayment — success activates items with matching rentalId ─────────

    [Fact]
    public async Task MakePayment_Success_OnlyActivatesItemsForThisRental()
    {
        var rental = new Rental { Id = 1, UserId = 1, TotalAmount = 50, Status = RentalStatus.PaymentPending };
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(rental);

        var item1 = new RentalItem { Id = 1, RentalId = 1, MovieId = 1, IsActive = false };
        var item2 = new RentalItem { Id = 2, RentalId = 2, MovieId = 2, IsActive = false }; // different rental
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem> { item1, item2 });
        _itemRepo.Setup(r => r.Update(It.IsAny<int>(), It.IsAny<RentalItem>()))
            .ReturnsAsync((int id, RentalItem i) => i);
        _payRepo.Setup(r => r.Add(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => { p.User = new User { Name = "u" }; return p; });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>())).ReturnsAsync(rental);

        _ctx.Users.Add(new User { Id = 1, Username = "u1", Role = "Customer" });
        await _ctx.SaveChangesAsync();

        await _sut.MakePayment(new MakePaymentRequestDto { RentalId = 1, IsSuccess = true });

        // Only item1 (RentalId=1) should be activated
        _itemRepo.Verify(r => r.Update(1, It.Is<RentalItem>(i => i.IsActive)), Times.Once);
        _itemRepo.Verify(r => r.Update(2, It.IsAny<RentalItem>()), Times.Never);
    }

    // ── GetPaymentsByRental — filters by rentalId ────────────────────────────

    [Fact]
    public async Task GetPaymentsByRental_MultiplePayments_ReturnsOnlyForRental()
    {
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(new Rental { Id = 1, UserId = 1 });
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment> { MakePayment(1), MakePayment(2, 2) });

        var result = (await _sut.GetPaymentsByRental(1)).ToList();
        Assert.Single(result);
        Assert.Equal(1, result[0].RentalId);
    }

    // ── GetPaymentByUser — filters by userId ─────────────────────────────────

    [Fact]
    public async Task GetPaymentByUser_MultipleUsers_ReturnsOnlyForUser()
    {
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment> { MakePayment(1, 1), MakePayment(2, 2) });

        var result = (await _sut.GetPaymentByUser(1)).ToList();
        Assert.Single(result);
        Assert.Equal(1, result[0].UserId);
    }

    // ── GetAllPayments — ordered by date descending ──────────────────────────

    [Fact]
    public async Task GetAllPayments_ReturnsOrderedByDateDesc()
    {
        var older = MakePayment(1);
        older.PaymentDate = DateTime.UtcNow.AddDays(-2);
        var newer = MakePayment(2);
        newer.Id = 2;
        newer.PaymentDate = DateTime.UtcNow;

        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment> { older, newer });

        var result = (await _sut.GetAllPayments()).ToList();
        Assert.True(result[0].PaymentDate >= result[1].PaymentDate);
    }
}
