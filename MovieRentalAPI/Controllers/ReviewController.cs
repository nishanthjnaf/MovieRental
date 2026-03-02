using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewController(IReviewService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewRequestDto request)
        {
            var result = await _service.AddReview(request);
            return Ok(result);
        }

        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetReviewsByMovie(int movieId)
        {
            var result = await _service.GetReviewsByMovie(movieId);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReviewsByUser(int userId)
        {
            var result = await _service.GetReviewsByUser(userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewRequestDto request)
        {
            var result = await _service.UpdateReview(id, request);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var success = await _service.DeleteReview(id);

            if (!success)
                return NotFound();

            return Ok("Review deleted");
        }
    }
}