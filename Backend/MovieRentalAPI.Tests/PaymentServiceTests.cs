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

public class PaymentServiceTests
{
    private readonly Mock<IRepository<int, Payment>> _payRepo = new();
    private readonly Mock<IRepository<int, Rental>> _rentalRepo = new();
    private readonly Mock<IRepository<int, RentalItem>> _itemRepo = new();
    private readonly Mock<IActivityLogService> _logService = new();
    private readonly MovieRentalContext _ctx;
    private readonly NotificationService _notifService;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        var opts = new DbContextOptionsBuilder<MovieRentalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _ctx = new MovieRentalContext(opts);
        _notifService = new NotificationService(_ctx);
        _logService.Setup(l => l.Log(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _sut = new PaymentService(_payRepo.Object, _rentalRepo.Object, _itemRepo.Object,
            _ctx, _notifService, _logService.Object);
    }

    private static Rental MakeRental(int id = 1, RentalStatus status = RentalStatus.PaymentPending) =>
        new() { Id = id, UserId = 1, TotalAmount = 100, Status = status };

    private static Payment MakePayment(int id = 1) => new()
    {
        Id = id, RentalId = 1, UserId = 1, Amount = 100,
        PaymentMethod = PaymentMethod.CreditCard,
        PaymentType = PaymentType.Purchase,
        Status = PaymentStatus.Success,
        PaymentId = "PAY_TEST",
        PaymentDate = DateTime.UtcNow,
        User = new User { Id = 1, Username = "u1", Name = "User One" }
    };

    // ── MakePayment — success ───────────────────────────────────────────────

    [Fact]
    public async Task MakePayment_Success_ReturnsSuccessDto()
    {
        var rental = MakeRental();
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(rental);
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem>());
        _payRepo.Setup(r => r.Add(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => { p.Id = 1; p.User = new User { Name = "u1" }; return p; });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>())).ReturnsAsync(rental);

        // Seed a user for notifications
        _ctx.Users.Add(new User { Id = 1, Username = "u1", Role = "Customer" });
        await _ctx.SaveChangesAsync();

        var result = await _sut.MakePayment(new MakePaymentRequestDto
        { RentalId = 1, Method = PaymentMethod.CreditCard, IsSuccess = true });

        Assert.Equal(PaymentStatus.Success, result.Status);
    }

    [Fact]
    public async Task MakePayment_Failure_ReturnsFailedDto()
    {
        var rental = MakeRental();
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(rental);
        _payRepo.Setup(r => r.Add(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => { p.Id = 1; p.User = new User { Name = "u1" }; return p; });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>())).ReturnsAsync(rental);

        _ctx.Users.Add(new User { Id = 1, Username = "u1", Role = "Customer" });
        await _ctx.SaveChangesAsync();

        var result = await _sut.MakePayment(new MakePaymentRequestDto
        { RentalId = 1, Method = PaymentMethod.UPI, IsSuccess = false });

        Assert.Equal(PaymentStatus.Failed, result.Status);
    }

    [Fact]
    public async Task MakePayment_RentalNotFound_ThrowsNotFound()
    {
        _rentalRepo.Setup(r => r.Get(99)).ReturnsAsync((Rental?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.MakePayment(new MakePaymentRequestDto { RentalId = 99, IsSuccess = true }));
    }

    [Fact]
    public async Task MakePayment_AlreadyProcessed_ThrowsConflict()
    {
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeRental(1, RentalStatus.Available));
        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.MakePayment(new MakePaymentRequestDto { RentalId = 1, IsSuccess = true }));
    }

    [Fact]
    public async Task MakePayment_Success_ActivatesRentalItems()
    {
        var rental = MakeRental();
        var item = new RentalItem { Id = 1, RentalId = 1, MovieId = 1, IsActive = false };
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(rental);
        _itemRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RentalItem> { item });
        _itemRepo.Setup(r => r.Update(1, It.IsAny<RentalItem>())).ReturnsAsync(item);
        _payRepo.Setup(r => r.Add(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => { p.User = new User { Name = "u1" }; return p; });
        _rentalRepo.Setup(r => r.Update(1, It.IsAny<Rental>())).ReturnsAsync(rental);

        _ctx.Users.Add(new User { Id = 1, Username = "u1", Role = "Customer" });
        await _ctx.SaveChangesAsync();

        await _sut.MakePayment(new MakePaymentRequestDto { RentalId = 1, IsSuccess = true });

        _itemRepo.Verify(r => r.Update(1, It.Is<RentalItem>(i => i.IsActive)), Times.Once);
    }

    // ── GetPaymentsByRental ─────────────────────────────────────────────────

    [Fact]
    public async Task GetPaymentsByRental_HasPayments_ReturnsAll()
    {
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeRental());
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment> { MakePayment() });

        var result = (await _sut.GetPaymentsByRental(1)).ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetPaymentsByRental_RentalNotFound_ThrowsNotFound()
    {
        _rentalRepo.Setup(r => r.Get(99)).ReturnsAsync((Rental?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetPaymentsByRental(99));
    }

    [Fact]
    public async Task GetPaymentsByRental_NoPayments_ThrowsNotFound()
    {
        _rentalRepo.Setup(r => r.Get(1)).ReturnsAsync(MakeRental());
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetPaymentsByRental(1));
    }

    // ── GetPaymentByUser ────────────────────────────────────────────────────

    [Fact]
    public async Task GetPaymentByUser_HasPayments_ReturnsAll()
    {
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment> { MakePayment() });

        var result = (await _sut.GetPaymentByUser(1)).ToList();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetPaymentByUser_NoPayments_ThrowsNotFound()
    {
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetPaymentByUser(99));
    }

    // ── GetAllPayments ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllPayments_HasPayments_ReturnsAll()
    {
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment> { MakePayment(), MakePayment(2) });

        var result = (await _sut.GetAllPayments()).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllPayments_Empty_ThrowsNotFound()
    {
        _payRepo.Setup(r => r.GetAllIncluding(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>[]>()))
            .ReturnsAsync(new List<Payment>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllPayments());
    }

    // ── GetItemRefund ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetItemRefund_NoRefund_ReturnsNull()
    {
        var result = await _sut.GetItemRefund(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemRefund_HasRefund_ReturnsDto()
    {
        _ctx.RentalItemRefunds.Add(new RentalItemRefund
        {
            Id = 1, RentalItemId = 1, RentalId = 1, UserId = 1,
            RefundAmount = 50, RefundedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync();

        var result = await _sut.GetItemRefund(1);
        Assert.NotNull(result);
        Assert.Equal(50, result!.RefundAmount);
    }
}
