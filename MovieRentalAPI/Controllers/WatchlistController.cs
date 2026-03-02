using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WatchlistController : ControllerBase
    {
        private readonly IWatchlistService _service;

        public WatchlistController(IWatchlistService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddToWatchlist(
            [FromBody] WatchlistRequestDto request)
        {
            var result = await _service.AddToWatchlist(request);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserWatchlist(int userId)
        {
            var result = await _service.GetUserWatchlist(userId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var success = await _service.RemoveFromWatchlist(id);

            if (!success)
                return NotFound();

            return Ok("Removed from watchlist");
        }
    }
}