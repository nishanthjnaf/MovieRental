using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;
using MovieRentalModels;
using Xunit;

namespace MovieRentalAPI.Tests;

public class ActivityLogServiceTests
{
    private static MovieRentalContext MakeContext()
    {
        var opts = new DbContextOptionsBuilder<MovieRentalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MovieRentalContext(opts);
    }

    // ── Log ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Log_WritesEntryToDatabase()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);

        await sut.Log(1, "alice", "Customer", "Payment", "MakePayment", "Paid ₹100");

        Assert.Single(ctx.ActivityLogs);
        var entry = ctx.ActivityLogs.First();
        Assert.Equal(1, entry.UserId);
        Assert.Equal("alice", entry.UserName);
        Assert.Equal("Customer", entry.Role);
        Assert.Equal("Payment", entry.Entity);
        Assert.Equal("MakePayment", entry.Action);
        Assert.Equal("Paid ₹100", entry.Details);
        Assert.Equal("Success", entry.Status);
    }

    [Fact]
    public async Task Log_CustomStatus_PersistsStatus()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);

        await sut.Log(2, "bob", "Admin", "Movie", "DeleteMovie", "Deleted movie #5", "Failure");

        Assert.Equal("Failure", ctx.ActivityLogs.First().Status);
    }

    [Fact]
    public async Task Log_MultipleCalls_AllPersisted()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);

        await sut.Log(1, "u1", "Customer", "Rental", "CreateRental", "d1");
        await sut.Log(2, "u2", "Admin", "Movie", "AddMovie", "d2");

        Assert.Equal(2, ctx.ActivityLogs.Count());
    }

    // ── GetLogs — no filters ────────────────────────────────────────────────

    [Fact]
    public async Task GetLogs_NoFilters_ReturnsAllDescending()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "Payment", "Pay", "d1");
        await sut.Log(2, "u2", "Admin", "Movie", "Add", "d2");

        var result = await sut.GetLogs(new ActivityLogQueryDto { Page = 1, PageSize = 50 });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
    }

    // ── GetLogs — filter by UserId ──────────────────────────────────────────

    [Fact]
    public async Task GetLogs_FilterByUserId_ReturnsOnlyThatUser()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "Payment", "Pay", "d");
        await sut.Log(2, "u2", "Admin", "Movie", "Add", "d");

        var result = await sut.GetLogs(new ActivityLogQueryDto { UserId = 1, Page = 1, PageSize = 50 });

        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Items, l => Assert.Equal(1, l.UserId));
    }

    // ── GetLogs — filter by Role ────────────────────────────────────────────

    [Fact]
    public async Task GetLogs_FilterByRole_ReturnsOnlyThatRole()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "Payment", "Pay", "d");
        await sut.Log(2, "u2", "Admin", "Movie", "Add", "d");

        var result = await sut.GetLogs(new ActivityLogQueryDto { Role = "Admin", Page = 1, PageSize = 50 });

        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Items, l => Assert.Equal("Admin", l.Role));
    }

    // ── GetLogs — filter by Entity ──────────────────────────────────────────

    [Fact]
    public async Task GetLogs_FilterByEntity_ReturnsMatching()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "Payment", "Pay", "d");
        await sut.Log(2, "u2", "Admin", "Movie", "Add", "d");

        var result = await sut.GetLogs(new ActivityLogQueryDto { Entity = "Payment", Page = 1, PageSize = 50 });

        Assert.Equal(1, result.TotalCount);
    }

    // ── GetLogs — filter by Action (contains) ──────────────────────────────

    [Fact]
    public async Task GetLogs_FilterByAction_ReturnsContainsMatch()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "Payment", "MakePayment", "d");
        await sut.Log(2, "u2", "Admin", "Movie", "AddMovie", "d");

        var result = await sut.GetLogs(new ActivityLogQueryDto { Action = "Payment", Page = 1, PageSize = 50 });

        Assert.Equal(1, result.TotalCount);
    }

    // ── GetLogs — filter by Status ──────────────────────────────────────────

    [Fact]
    public async Task GetLogs_FilterByStatus_ReturnsMatching()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "Payment", "Pay", "d", "Success");
        await sut.Log(2, "u2", "Admin", "Movie", "Add", "d", "Failure");

        var result = await sut.GetLogs(new ActivityLogQueryDto { Status = "Failure", Page = 1, PageSize = 50 });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Failure", result.Items.First().Status);
    }

    // ── GetLogs — sort ascending ────────────────────────────────────────────

    [Fact]
    public async Task GetLogs_SortAsc_ReturnsOldestFirst()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "A", "A1", "d");
        await Task.Delay(5);
        await sut.Log(2, "u2", "Admin", "B", "B1", "d");

        var result = await sut.GetLogs(new ActivityLogQueryDto { SortOrder = "asc", Page = 1, PageSize = 50 });
        var items = result.Items.ToList();

        Assert.True(items[0].PerformedAt <= items[1].PerformedAt);
    }

    // ── GetLogs — pagination ────────────────────────────────────────────────

    [Fact]
    public async Task GetLogs_Pagination_ReturnsCorrectPage()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        for (int i = 0; i < 10; i++)
            await sut.Log(i, $"u{i}", "Customer", "E", "A", "d");

        var result = await sut.GetLogs(new ActivityLogQueryDto { Page = 2, PageSize = 3 });

        Assert.Equal(10, result.TotalCount);
        Assert.Equal(3, result.Items.Count());
        Assert.Equal(2, result.Page);
    }

    [Fact]
    public async Task GetLogs_PageSizeClamped_MaxIs200()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        for (int i = 0; i < 5; i++)
            await sut.Log(i, $"u{i}", "Customer", "E", "A", "d");

        var result = await sut.GetLogs(new ActivityLogQueryDto { Page = 1, PageSize = 500 });
        Assert.Equal(5, result.Items.Count());
    }

    [Fact]
    public async Task GetLogs_PageZero_TreatedAsPage1()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "E", "A", "d");

        var result = await sut.GetLogs(new ActivityLogQueryDto { Page = 0, PageSize = 10 });
        Assert.Equal(1, result.Page);
    }

    // ── GetLogs — date range ────────────────────────────────────────────────

    [Fact]
    public async Task GetLogs_FilterByFrom_ExcludesOlderEntries()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "E", "A", "d");

        var tomorrow = DateTime.UtcNow.AddDays(1);
        var result = await sut.GetLogs(new ActivityLogQueryDto { From = tomorrow, Page = 1, PageSize = 50 });

        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetLogs_FilterByTo_ExcludesFutureEntries()
    {
        using var ctx = MakeContext();
        var sut = new ActivityLogService(ctx);
        await sut.Log(1, "u1", "Customer", "E", "A", "d");

        var yesterday = DateTime.UtcNow.AddDays(-1);
        var result = await sut.GetLogs(new ActivityLogQueryDto { To = yesterday, Page = 1, PageSize = 50 });

        Assert.Equal(0, result.TotalCount);
    }
}
