using MovieRentalAPI.Services;
using Xunit;

namespace MovieRentalAPI.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _sut = new();

    // ── HashPassword — registration (no existing key) ───────────────────────

    [Fact]
    public void HashPassword_NewUser_ReturnsHashAndKey()
    {
        var hash = _sut.HashPassword("password123", null, out var key);
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(key);
        Assert.NotEmpty(key);
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        var hash1 = _sut.HashPassword("password123", null, out var key1);
        var hash2 = _sut.HashPassword("password123", null, out var key2);
        // Different keys → different hashes
        Assert.False(hash1.SequenceEqual(hash2));
    }

    // ── HashPassword — login (existing key) ────────────────────────────────

    [Fact]
    public void HashPassword_WithExistingKey_ProducesSameHash()
    {
        var original = _sut.HashPassword("mypassword", null, out var key);
        var login = _sut.HashPassword("mypassword", key, out _);
        Assert.True(original.SequenceEqual(login));
    }

    [Fact]
    public void HashPassword_WrongPassword_ProducesDifferentHash()
    {
        var original = _sut.HashPassword("correct", null, out var key);
        var wrong = _sut.HashPassword("wrong", key, out _);
        Assert.False(original.SequenceEqual(wrong));
    }

    [Fact]
    public void HashPassword_WithExistingKey_KeyOutIsNull()
    {
        _sut.HashPassword("pass", null, out var key);
        _sut.HashPassword("pass", key, out var outKey);
        Assert.Null(outKey);
    }

    // ── HashPassword — edge cases ───────────────────────────────────────────

    [Fact]
    public void HashPassword_EmptyPassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.HashPassword("", null, out _));
    }

    [Fact]
    public void HashPassword_NullPassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.HashPassword(null!, null, out _));
    }

    [Fact]
    public void HashPassword_LongPassword_ReturnsHash()
    {
        var longPass = new string('a', 1000);
        var hash = _sut.HashPassword(longPass, null, out var key);
        Assert.NotNull(hash);
        Assert.NotNull(key);
    }

    [Fact]
    public void HashPassword_SpecialCharacters_ReturnsHash()
    {
        var hash = _sut.HashPassword("P@$$w0rd!#%^&*()", null, out var key);
        Assert.NotNull(hash);
        Assert.NotNull(key);
    }

    [Fact]
    public void HashPassword_UnicodePassword_ReturnsHash()
    {
        var hash = _sut.HashPassword("パスワード123", null, out var key);
        Assert.NotNull(hash);
        Assert.NotNull(key);
    }
}
