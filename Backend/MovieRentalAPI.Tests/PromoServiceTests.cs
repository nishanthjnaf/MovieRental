using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;
using Xunit;

namespace MovieRentalAPI.Tests;

public class PromoServiceTests
{
    private readonly PromoService _sut = new();

    // ── GetAll ──────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_ReturnsAllPromos()
    {
        var result = _sut.GetAll().ToList();
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Code == "DOUBLE5");
        Assert.Contains(result, p => p.Code == "TRIPLE10");
    }

    [Fact]
    public void GetAll_PromoHasCorrectDiscountValues()
    {
        var promos = _sut.GetAll().ToList();
        var double5 = promos.First(p => p.Code == "DOUBLE5");
        var triple10 = promos.First(p => p.Code == "TRIPLE10");

        Assert.Equal(5, double5.DiscountPct);
        Assert.Equal(2, double5.MinItems);
        Assert.Equal(10, triple10.DiscountPct);
        Assert.Equal(3, triple10.MinItems);
    }

    // ── Apply — invalid code ────────────────────────────────────────────────

    [Fact]
    public void Apply_InvalidCode_ReturnsInvalid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "BADCODE", ItemCount = 5 });
        Assert.False(result.IsValid);
        Assert.Equal("Invalid promo code", result.Message);
    }

    [Fact]
    public void Apply_EmptyCode_ReturnsInvalid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "", ItemCount = 5 });
        Assert.False(result.IsValid);
    }

    // ── Apply — not enough items ────────────────────────────────────────────

    [Fact]
    public void Apply_DOUBLE5_OneItem_ReturnsInvalid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "DOUBLE5", ItemCount = 1 });
        Assert.False(result.IsValid);
        Assert.Contains("1 more movie", result.Message);
    }

    [Fact]
    public void Apply_TRIPLE10_TwoItems_ReturnsInvalid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "TRIPLE10", ItemCount = 2 });
        Assert.False(result.IsValid);
        Assert.Contains("1 more movie", result.Message);
    }

    // ── Apply — success ─────────────────────────────────────────────────────

    [Fact]
    public void Apply_DOUBLE5_TwoItems_ReturnsValid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "DOUBLE5", ItemCount = 2 });
        Assert.True(result.IsValid);
        Assert.Equal("DOUBLE5", result.Code);
        Assert.Equal(5, result.DiscountPct);
    }

    [Fact]
    public void Apply_TRIPLE10_ThreeItems_ReturnsValid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "TRIPLE10", ItemCount = 3 });
        Assert.True(result.IsValid);
        Assert.Equal("TRIPLE10", result.Code);
        Assert.Equal(10, result.DiscountPct);
    }

    [Fact]
    public void Apply_DOUBLE5_MoreThanMinItems_ReturnsValid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "DOUBLE5", ItemCount = 10 });
        Assert.True(result.IsValid);
    }

    // ── Apply — case insensitive ────────────────────────────────────────────

    [Fact]
    public void Apply_LowercaseCode_ReturnsValid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "double5", ItemCount = 2 });
        Assert.True(result.IsValid);
        Assert.Equal("DOUBLE5", result.Code);
    }

    [Fact]
    public void Apply_MixedCaseCode_ReturnsValid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "Triple10", ItemCount = 3 });
        Assert.True(result.IsValid);
    }

    // ── Apply — zero items ──────────────────────────────────────────────────

    [Fact]
    public void Apply_ZeroItems_ReturnsInvalid()
    {
        var result = _sut.Apply(new ApplyPromoRequestDto { Code = "DOUBLE5", ItemCount = 0 });
        Assert.False(result.IsValid);
    }
}
