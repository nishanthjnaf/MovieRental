using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
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
        public async Task<IActionResult> AddToWatchlist([FromBody] WatchlistRequestDto request)
        {
            try
            {
                var result = await _service.AddToWatchlist(request);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserWatchlist(int userId)
        {
            try
            {
                var result = await _service.GetUserWatchlist(userId);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                await _service.RemoveFromWatchlist(id);
                return Ok("Removed from watchlist successfully");
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
