using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Models;
using MovieRentalAPI.Services;
using MovieRentalModels;
using Xunit;

namespace MovieRentalAPI.Tests;

public class NotificationServiceTests
{
    private static MovieRentalContext MakeCtx() =>
        new(new DbContextOptionsBuilder<MovieRentalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    // ── Push ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Push_CreatesNotification()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);

        await sut.Push(1, "payment", "Title", "Message", 42);

        var n = ctx.Notifications.Single();
        Assert.Equal(1, n.UserId);
        Assert.Equal("payment", n.Type);
        Assert.Equal("Title", n.Title);
        Assert.Equal("Message", n.Message);
        Assert.Equal(42, n.RelatedId);
        Assert.False(n.IsRead);
    }

    [Fact]
    public async Task Push_WithoutRelatedId_CreatesNotification()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        await sut.Push(2, "info", "T", "M");
        Assert.Single(ctx.Notifications);
        Assert.Null(ctx.Notifications.First().RelatedId);
    }

    // ── Broadcast ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Broadcast_SendsToAllCustomers()
    {
        using var ctx = MakeCtx();
        ctx.Users.AddRange(
            new User { Id = 1, Username = "c1", Role = "Customer" },
            new User { Id = 2, Username = "c2", Role = "Customer" },
            new User { Id = 3, Username = "admin", Role = "Admin" }
        );
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        var result = await sut.Broadcast("promo", "Sale", "Big sale!", 3, "admin");

        // 2 customer notifications + 1 broadcast record
        Assert.Equal(2, ctx.Notifications.Count());
        Assert.Equal("Sale", result.Title);
        Assert.Equal("admin", result.SentByUsername);
    }

    [Fact]
    public async Task Broadcast_NoCustomers_CreatesOnlyRecord()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        var result = await sut.Broadcast("info", "T", "M");
        Assert.Empty(ctx.Notifications);
        Assert.Single(ctx.BroadcastMessages);
    }

    // ── GetAllBroadcasts ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllBroadcasts_ReturnsOrderedByDate()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        await sut.Broadcast("t", "First", "M");
        await Task.Delay(5);
        await sut.Broadcast("t", "Second", "M");

        var result = (await sut.GetAllBroadcasts()).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal("Second", result[0].Title); // newest first
    }

    [Fact]
    public async Task GetAllBroadcasts_Empty_ReturnsEmpty()
    {
        using var ctx = MakeCtx();
        var result = await new NotificationService(ctx).GetAllBroadcasts();
        Assert.Empty(result);
    }

    // ── DeleteBroadcast ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteBroadcast_Exists_Removes()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        var dto = await sut.Broadcast("t", "T", "M");
        await sut.DeleteBroadcast(dto.Id);
        Assert.Empty(ctx.BroadcastMessages);
    }

    [Fact]
    public async Task DeleteBroadcast_NotFound_DoesNotThrow()
    {
        using var ctx = MakeCtx();
        var ex = await Record.ExceptionAsync(() => new NotificationService(ctx).DeleteBroadcast(999));
        Assert.Null(ex);
    }

    // ── GetSenderUsername ───────────────────────────────────────────────────

    [Fact]
    public async Task GetSenderUsername_Exists_ReturnsUsername()
    {
        using var ctx = MakeCtx();
        ctx.Users.Add(new User { Id = 1, Username = "alice", Role = "Admin" });
        await ctx.SaveChangesAsync();

        var result = await new NotificationService(ctx).GetSenderUsername(1);
        Assert.Equal("alice", result);
    }

    [Fact]
    public async Task GetSenderUsername_NotFound_ReturnsAdmin()
    {
        using var ctx = MakeCtx();
        var result = await new NotificationService(ctx).GetSenderUsername(999);
        Assert.Equal("Admin", result);
    }

    // ── GetForUser ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetForUser_ReturnsOnlyUserNotifications()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        await sut.Push(1, "t", "T1", "M");
        await sut.Push(2, "t", "T2", "M");

        var result = (await sut.GetForUser(1)).ToList();
        Assert.Single(result);
        Assert.Equal(1, result[0].UserId);
    }

    [Fact]
    public async Task GetForUser_NoNotifications_ReturnsEmpty()
    {
        using var ctx = MakeCtx();
        var result = await new NotificationService(ctx).GetForUser(99);
        Assert.Empty(result);
    }

    // ── GetUnreadCount ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetUnreadCount_ReturnsCorrectCount()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        await sut.Push(1, "t", "T1", "M");
        await sut.Push(1, "t", "T2", "M");

        Assert.Equal(2, await sut.GetUnreadCount(1));
    }

    // ── MarkRead ────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkRead_MarksNotificationAsRead()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        await sut.Push(1, "t", "T", "M");
        var id = ctx.Notifications.First().Id;

        await sut.MarkRead(id);
        Assert.True(ctx.Notifications.First().IsRead);
    }

    [Fact]
    public async Task MarkRead_NotFound_DoesNotThrow()
    {
        using var ctx = MakeCtx();
        var ex = await Record.ExceptionAsync(() => new NotificationService(ctx).MarkRead(999));
        Assert.Null(ex);
    }

    // ── MarkAllRead ─────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAllRead_MarksAllForUser()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        await sut.Push(1, "t", "T1", "M");
        await sut.Push(1, "t", "T2", "M");
        await sut.Push(2, "t", "T3", "M");

        await sut.MarkAllRead(1);

        Assert.Equal(0, await sut.GetUnreadCount(1));
        Assert.Equal(1, await sut.GetUnreadCount(2)); // user 2 unaffected
    }

    // ── Delete ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Exists_Removes()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        await sut.Push(1, "t", "T", "M");
        var id = ctx.Notifications.First().Id;

        await sut.Delete(id);
        Assert.Empty(ctx.Notifications);
    }

    [Fact]
    public async Task Delete_NotFound_DoesNotThrow()
    {
        using var ctx = MakeCtx();
        var ex = await Record.ExceptionAsync(() => new NotificationService(ctx).Delete(999));
        Assert.Null(ex);
    }

    // ── NotifyNewMovie ──────────────────────────────────────────────────────

    [Fact]
    public async Task NotifyNewMovie_MatchingPreference_SendsNotification()
    {
        using var ctx = MakeCtx();
        ctx.UserPreferences.Add(new UserPreference
        { UserId = 1, PreferredGenres = "Action", PreferredLanguages = "English", IsSet = true });
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        await sut.NotifyNewMovie(10, "Action Film", new[] { "Action" }, "English");

        Assert.Single(ctx.Notifications);
        Assert.Equal(1, ctx.Notifications.First().UserId);
    }

    [Fact]
    public async Task NotifyNewMovie_NoMatchingPreference_NoNotification()
    {
        using var ctx = MakeCtx();
        ctx.UserPreferences.Add(new UserPreference
        { UserId = 1, PreferredGenres = "Drama", PreferredLanguages = "French", IsSet = true });
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        await sut.NotifyNewMovie(10, "Action Film", new[] { "Action" }, "English");

        Assert.Empty(ctx.Notifications);
    }

    [Fact]
    public async Task NotifyNewMovie_NoPreferences_NoNotification()
    {
        using var ctx = MakeCtx();
        var sut = new NotificationService(ctx);
        await sut.NotifyNewMovie(10, "Film", new[] { "Action" }, "English");
        Assert.Empty(ctx.Notifications);
    }

    [Fact]
    public async Task NotifyNewMovie_EmptyGenrePreference_MatchesAnyGenre()
    {
        using var ctx = MakeCtx();
        ctx.UserPreferences.Add(new UserPreference
        { UserId = 1, PreferredGenres = "", PreferredLanguages = "English", IsSet = true });
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        await sut.NotifyNewMovie(10, "Film", new[] { "Action" }, "English");

        Assert.Single(ctx.Notifications);
    }

    // ── CheckExpiringRentals ────────────────────────────────────────────────

    [Fact]
    public async Task CheckExpiringRentals_ExpiringItem_SendsNotification()
    {
        using var ctx = MakeCtx();
        var movie = new Movie { Id = 1, Title = "Film", Language = "En", Genres = new List<Genre>() };
        var rental = new Rental { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.Available };
        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddHours(12) // expires in 12h — within 24h window
        };
        ctx.Set<Movie>().Add(movie);
        ctx.Set<Rental>().Add(rental);
        ctx.Set<RentalItem>().Add(item);
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        await sut.CheckExpiringRentals();

        Assert.Single(ctx.Notifications);
        Assert.Equal("expiry", ctx.Notifications.First().Type);
    }

    [Fact]
    public async Task CheckExpiringRentals_AlreadyNotified_NoDuplicate()
    {
        using var ctx = MakeCtx();
        var movie = new Movie { Id = 1, Title = "Film", Language = "En", Genres = new List<Genre>() };
        var rental = new Rental { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.Available };
        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddHours(12)
        };
        ctx.Set<Movie>().Add(movie);
        ctx.Set<Rental>().Add(rental);
        ctx.Set<RentalItem>().Add(item);
        // Pre-existing expiry notification
        ctx.Notifications.Add(new Notification
        {
            UserId = 1, Type = "expiry", RelatedId = 1,
            CreatedAt = DateTime.UtcNow.AddHours(-1), IsRead = false, Title = "T", Message = "M"
        });
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        await sut.CheckExpiringRentals();

        Assert.Single(ctx.Notifications); // no new one added
    }

    // ── CheckExpiredRentals ─────────────────────────────────────────────────

    [Fact]
    public async Task CheckExpiredRentals_JustExpiredItem_SendsNotificationAndDeactivates()
    {
        using var ctx = MakeCtx();
        var movie = new Movie { Id = 1, Title = "Film", Language = "En", Genres = new List<Genre>() };
        var rental = new Rental { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.Available };
        // Use IST-aware time: add 5h30m offset to ensure it falls in the 30-min window
        var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = true,
            StartDate = istNow.AddDays(-3),
            EndDate = istNow.AddMinutes(-5) // expired 5 min ago in IST
        };
        ctx.Set<Movie>().Add(movie);
        ctx.Set<Rental>().Add(rental);
        ctx.Set<RentalItem>().Add(item);
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        await sut.CheckExpiredRentals();

        Assert.Single(ctx.Notifications);
        Assert.Equal("expired", ctx.Notifications.First().Type);
        Assert.False(ctx.Set<RentalItem>().First().IsActive);
    }
}

public class NotificationServiceCoverageTests
{
    private static MovieRentalContext MakeCtx() =>
        new(new DbContextOptionsBuilder<MovieRentalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    // ── CheckExpiringRentals — rental not found ──────────────────────────────

    [Fact]
    public async Task CheckExpiringRentals_RentalNotFound_SkipsItem()
    {
        using var ctx = MakeCtx();
        var movie = new Movie { Id = 1, Title = "Film", Language = "En", Genres = new List<Genre>() };
        // Item references rental 99 which doesn't exist
        var item = new RentalItem
        {
            Id = 1, RentalId = 99, MovieId = 1, IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddHours(12)
        };
        ctx.Set<Movie>().Add(movie);
        ctx.Set<RentalItem>().Add(item);
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        await sut.CheckExpiringRentals(); // should not throw

        Assert.Empty(ctx.Notifications);
    }

    // ── CheckExpiringRentals — no expiring items ─────────────────────────────

    [Fact]
    public async Task CheckExpiringRentals_NoExpiringItems_NoNotifications()
    {
        using var ctx = MakeCtx();
        // Item expires in 48h — outside the 24h window
        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = true,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(48)
        };
        ctx.Set<RentalItem>().Add(item);
        await ctx.SaveChangesAsync();

        await new NotificationService(ctx).CheckExpiringRentals();
        Assert.Empty(ctx.Notifications);
    }

    // ── CheckExpiredRentals — rental not found ───────────────────────────────

    [Fact]
    public async Task CheckExpiredRentals_RentalNotFound_SkipsItem()
    {
        using var ctx = MakeCtx();
        var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
        var item = new RentalItem
        {
            Id = 1, RentalId = 999, MovieId = 1, IsActive = true,
            StartDate = istNow.AddDays(-3),
            EndDate = istNow.AddMinutes(-5)
        };
        ctx.Set<RentalItem>().Add(item);
        await ctx.SaveChangesAsync();

        await new NotificationService(ctx).CheckExpiredRentals();
        Assert.Empty(ctx.Notifications);
    }

    // ── CheckExpiredRentals — already notified ───────────────────────────────

    [Fact]
    public async Task CheckExpiredRentals_AlreadyNotified_NoDuplicate()
    {
        using var ctx = MakeCtx();
        var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
        var rental = new Rental { Id = 1, UserId = 1, Status = MovieRentalAPI.Models.Enums.RentalStatus.Available };
        var item = new RentalItem
        {
            Id = 1, RentalId = 1, MovieId = 1, IsActive = true,
            StartDate = istNow.AddDays(-3),
            EndDate = istNow.AddMinutes(-5)
        };
        ctx.Set<Rental>().Add(rental);
        ctx.Set<RentalItem>().Add(item);
        // Pre-existing expired notification for this item
        ctx.Notifications.Add(new Notification
        {
            UserId = 1, Type = "expired", RelatedId = 1,
            CreatedAt = DateTime.UtcNow, IsRead = false, Title = "T", Message = "M"
        });
        await ctx.SaveChangesAsync();

        await new NotificationService(ctx).CheckExpiredRentals();
        Assert.Single(ctx.Notifications); // no new one
    }

    // ── NotifyNewMovie — empty language preference matches any ───────────────

    [Fact]
    public async Task NotifyNewMovie_EmptyLanguagePreference_MatchesAnyLanguage()
    {
        using var ctx = MakeCtx();
        ctx.UserPreferences.Add(new UserPreference
        { UserId = 1, PreferredGenres = "Action", PreferredLanguages = "", IsSet = true });
        await ctx.SaveChangesAsync();

        await new NotificationService(ctx).NotifyNewMovie(1, "Film", new[] { "Action" }, "Tamil");
        Assert.Single(ctx.Notifications);
    }

    // ── Broadcast — with relatedId ───────────────────────────────────────────

    [Fact]
    public async Task Broadcast_WithRelatedId_SetsRelatedId()
    {
        using var ctx = MakeCtx();
        ctx.Users.Add(new User { Id = 1, Username = "c1", Role = "Customer" });
        await ctx.SaveChangesAsync();

        var sut = new NotificationService(ctx);
        await sut.Broadcast("promo", "T", "M", relatedId: 42);

        Assert.Equal(42, ctx.Notifications.First().RelatedId);
    }
}
