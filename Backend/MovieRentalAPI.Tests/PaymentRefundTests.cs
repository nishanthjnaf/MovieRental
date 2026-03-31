using Microsoft.EntityFrameworkCore;
using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Models.Enums;
using MovieRentalAPI.Services;
using MovieRentalModels;
using Xunit;

namespace MovieRentalAPI.Tests;

/// <summary>
/// Focused tests for PaymentService.ProcessRefund which needs a real EF context
/// for RentalItemRefunds and Movies.
/// </summary>
public class PaymentRefundTests
{
    private readonly Mock<IRepository<int, Payment>> _payRepo = new();
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _itemRepo = new();
    private readonly Mock<IActivityLogService> _logService = new();
    private readonly MovieRentalContext _ctx;
    private readonly PaymentService _sut;

    public PaymentRefundTests()
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

    private static readonly TimeZoneInfo Ist =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    private async Task<(RentalItem item, Payment purchase)> SeedRefundScenario(
        double hoursSincePurchase = 1)
    {
        var user = new User { Id = 1, Username = "u1", Role = "Customer" };
        var movie = new Movie { Id = 1, Title = "Film", Language = "En", Genres = new List<Genre>() };
        _ctx.Users.Add(user);
        _ctx.Set<Movie>().Add(movie);
        await _ctx.SaveChangesAsync();

        var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);

        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = true,
            PricePerDay = 10,
            StartDate = istNow.AddDays(-3),
            EndDate = istNow.AddDays(4)  // 7 days total
        };
        _itemRepo.Setup(r => r.Get(1)).ReturnsAsync(item);
        _itemRepo.Setup(r => r.Update(1, It.IsAny<RentalItem>())).ReturnsAsync(item);

        var rental = new Rental { Id = 1, UserId = 1, TotalAmount = 70, Status = RentalStatus.Available };
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(rental);

        var purchase = new Payment
        {
            Id = 1, RentalId = 1, UserId = 1, Amount = 70,
            PaymentMethod = PaymentMethod.CreditCard,
            PaymentType = PaymentType.Purchase,
            Status = PaymentStatus.Success,
            PaymentId = "PAY_TEST",
            PaymentDate = istNow.AddHours(-hoursSincePurchase), // IST-based
            User = user
        };
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment> { purchase });
        _payRepo.Setup(r => r.Add(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => { p.User = user; return p; });

        return (item, purchase);
    }

    // ── ProcessRefund — item not found ──────────────────────────────────────

    [Fact]
    public async Task ProcessRefund_ItemNotFound_ThrowsNotFound()
    {
        _itemRepo.Setup(r => r.Get(99)).ReturnsAsync((RentalItem?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ProcessRefund(99));
    }

    // ── ProcessRefund — rental not found ────────────────────────────────────

    [Fact]
    public async Task ProcessRefund_RentalNotFound_ThrowsNotFound()
    {
        _itemRepo.Setup(r => r.Get(1)).ReturnsAsync(new RentalItem { Id = 1, RentalId = 99, MovieId = 1 });
        _rentalRepo.Setup(r => r.Get(99)).ReturnsAsync((Rental?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ProcessRefund(1));
    }

    // ── ProcessRefund — no purchase payment ─────────────────────────────────

    [Fact]
    public async Task ProcessRefund_NoPurchasePayment_ThrowsNotFound()
    {
        _itemRepo.Setup(r => r.Get(1)).ReturnsAsync(new RentalItem { Id = 1, RentalId = 1, MovieId = 1 });
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(new Rental { Id = 1, UserId = 1 });
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ProcessRefund(1));
    }

    // ── ProcessRefund — payment not eligible ────────────────────────────────

    [Fact]
    public async Task ProcessRefund_PaymentFailed_ThrowsConflict()
    {
        _itemRepo.Setup(r => r.Get(1)).ReturnsAsync(new RentalItem { Id = 1, RentalId = 1, MovieId = 1 });
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(new Rental { Id = 1, UserId = 1 });
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment>
            {
                new() { RentalId = 1, PaymentType = PaymentType.Purchase, Status = PaymentStatus.Failed,
                        PaymentDate = DateTime.UtcNow, User = new User { Id = 1, Username = "u" } }
            });
        await Assert.ThrowsAsync<ConflictException>(() => _sut.ProcessRefund(1));
    }

    // ── ProcessRefund — already refunded ────────────────────────────────────

    [Fact]
    public async Task ProcessRefund_AlreadyRefunded_ThrowsConflict()
    {
        await SeedRefundScenario();
        // Pre-seed a refund record
        _ctx.RentalItemRefunds.Add(new RentalItemRefund
        { RentalItemId = 1, RentalId = 1, UserId = 1, RefundAmount = 50, RefundedAt = DateTime.UtcNow });
        await _ctx.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => _sut.ProcessRefund(1));
    }

    // ── ProcessRefund — within 2 hours (75% refund) ─────────────────────────

    [Fact]
    public async Task ProcessRefund_Within2Hours_CreatesRefundRecord()
    {
        await SeedRefundScenario(hoursSincePurchase: 1);

        await _sut.ProcessRefund(1);
        // Refund record is always written regardless of amount
        Assert.Single(_ctx.RentalItemRefunds);
    }

    // ── ProcessRefund — between 2-4 hours (50% refund) ──────────────────────

    [Fact]
    public async Task ProcessRefund_Between2And4Hours_CreatesRefundRecord()
    {
        await SeedRefundScenario(hoursSincePurchase: 3);

        await _sut.ProcessRefund(1);
        Assert.Single(_ctx.RentalItemRefunds);
    }

    // ── ProcessRefund — after 4 hours (0% refund) ───────────────────────────

    [Fact]
    public async Task ProcessRefund_After4Hours_ZeroRefundRecord()
    {
        await SeedRefundScenario(hoursSincePurchase: 5);

        await _sut.ProcessRefund(1);
        Assert.Single(_ctx.RentalItemRefunds);
        Assert.Equal(0, _ctx.RentalItemRefunds.First().RefundAmount);
    }
}
