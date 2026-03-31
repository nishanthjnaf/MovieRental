using Microsoft.Extensions.Configuration;
using MovieRentalAPI.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace MovieRentalAPI.Tests;

public class TokenServiceTests
{
    private static TokenService MakeSut(string key = "super_secret_key_for_testing_1234567890!!")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpireMinutes"] = "60"
            })
            .Build();
        return new TokenService(config);
    }

    private static TokenPayloadDto MakePayload(int id = 1, string username = "alice", string role = "Customer") =>
        new() { UserId = id, Username = username, Role = role };

    // ── CreateToken — basic ─────────────────────────────────────────────────

    [Fact]
    public void CreateToken_ValidPayload_ReturnsNonEmptyString()
    {
        var token = MakeSut().CreateToken(MakePayload());
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void CreateToken_ReturnsValidJwt()
    {
        var token = MakeSut().CreateToken(MakePayload());
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));
    }

    [Fact]
    public void CreateToken_ContainsCorrectUsername()
    {
        var token = MakeSut().CreateToken(MakePayload(username: "bob"));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(jwt.Claims, c => c.Value == "bob");
    }

    [Fact]
    public void CreateToken_ContainsCorrectRole()
    {
        var token = MakeSut().CreateToken(MakePayload(role: "Admin"));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(jwt.Claims, c => c.Value == "Admin");
    }

    [Fact]
    public void CreateToken_ContainsCorrectUserId()
    {
        var token = MakeSut().CreateToken(MakePayload(id: 42));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(jwt.Claims, c => c.Value == "42");
    }

    [Fact]
    public void CreateToken_HasFutureExpiry()
    {
        var token = MakeSut().CreateToken(MakePayload());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public void CreateToken_DifferentPayloads_ProduceDifferentTokens()
    {
        var sut = MakeSut();
        var t1 = sut.CreateToken(MakePayload(username: "alice"));
        var t2 = sut.CreateToken(MakePayload(username: "bob"));
        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void CreateToken_AdminRole_ReturnsToken()
    {
        var token = MakeSut().CreateToken(MakePayload(role: "Admin"));
        Assert.NotEmpty(token);
    }
}
