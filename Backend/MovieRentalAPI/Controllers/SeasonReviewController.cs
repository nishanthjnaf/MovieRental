using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonReviewController : ControllerBase
    {
        private readonly ISeasonReviewService _service;

        public SeasonReviewController(ISeasonReviewService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(SeasonReviewRequestDto request)
        {
            try { return Ok(await _service.AddReview(request)); }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("season/{seasonId}")]
        public async Task<IActionResult> GetBySeason(int seasonId)
        {
            try { return Ok(await _service.GetReviewsBySeason(seasonId)); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try { return Ok(await _service.GetReviewsByUser(userId)); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try { await _service.DeleteReview(id); return Ok("Review deleted"); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
