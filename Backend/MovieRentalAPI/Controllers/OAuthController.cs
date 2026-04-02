using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalModels;
using System.Security.Claims;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        private readonly MovieRentalContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public OAuthController(MovieRentalContext context, ITokenService tokenService, IConfiguration config)
        {
            _context = context;
            _tokenService = tokenService;
            _config = config;
        }

        // ── Step 1: Redirect browser to Google's login page ─────────────────
        [HttpGet("login/google")]
        public IActionResult LoginWithGoogle([FromQuery] string returnUrl = "/")
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback)),
                Items = { { "returnUrl", returnUrl } }
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        // ── Step 2: Google redirects back here with the user's profile ───────
        [HttpGet("callback/google")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Cookies");
            if (!result.Succeeded)
                return Redirect(BuildFrontendUrl(null, "Google login failed"));

            var claims = result.Principal!.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
            var name  = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? email;

            var token = await FindOrCreateUser(email, name);
            return Redirect(BuildFrontendUrl(token, null));
        }

        // ── Shared: find existing user by email or auto-create one ───────────
        private async Task<string> FindOrCreateUser(string email, string name)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Username     = GenerateUsername(email),
                    Name         = name,
                    Email        = email,
                    Phone        = "",
                    Role         = "Customer",
                    Status       = "Active",
                    Password     = Array.Empty<byte>(),
                    PasswordHash = Array.Empty<byte>()
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return _tokenService.CreateToken(new TokenPayloadDto
            {
                UserId   = user.Id,
                Username = user.Username,
                Role     = user.Role
            });
        }

        // ── Build the frontend redirect URL with token or error ──────────────
        private string BuildFrontendUrl(string? token, string? error)
        {
            var frontendBase = _config["Frontend:BaseUrl"] ?? "http://localhost:4200";
            if (error != null)
                return $"{frontendBase}/login?error={Uri.EscapeDataString(error)}";
            return $"{frontendBase}/oauth-callback?token={token}";
        }

        // ── Generate a clean username from email ─────────────────────────────
        private static string GenerateUsername(string email)
        {
            var baseName = email.Split('@')[0]
                .ToLower()
                .Replace(".", "_")
                .Replace("+", "_");
            return $"{baseName}_{new Random().Next(100, 999)}";
        }
    }
}
