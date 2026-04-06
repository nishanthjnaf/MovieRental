using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeriesWatchlistController : ControllerBase
    {
        private readonly ISeriesWatchlistService _service;

        public SeriesWatchlistController(ISeriesWatchlistService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Add(SeriesWatchlistRequestDto request)
        {
            try { return Ok(await _service.AddToWatchlist(request)); }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try { return Ok(await _service.GetUserWatchlist(userId)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            try { await _service.RemoveFromWatchlist(id); return Ok("Removed"); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
