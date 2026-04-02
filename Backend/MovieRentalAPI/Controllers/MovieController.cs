using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Movie")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieServices _movieService;

        public MovieController(IMovieServices movieService)
        {
            _movieService = movieService;
        }

        [HttpPost]
        public async Task<IActionResult> AddMovie(CreateMovieRequestDto request)
        {
            try
            {
                var result = await _movieService.AddMovie(request);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovie(int id)
        {
            try
            {
                var result = await _movieService.GetMovieById(id);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            try
            {
                var result = await _movieService.GetAllMovies();
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchMovies([FromQuery] MovieSearchRequestDto request)
        {
            try
            {
                var result = await _movieService.SearchMovies(request);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(int id, CreateMovieRequestDto request)
        {
            try
            {
                var result = await _movieService.UpdateMovie(id, request);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                await _movieService.DeleteMovie(id);
                return Ok("Movie deleted successfully");
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("top-rented")]
        public async Task<IActionResult> GetTopRentedMovies([FromQuery] int count = 5)
        {
            try
            {
                var result = await _movieService.GetTopRentedMovies(count);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("top-user-rated")]
        public async Task<IActionResult> GetTopUserRatedMovies([FromQuery] int count = 10)
        {
            try
            {
                var result = await _movieService.GetTopUserRatedMovies(count);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("suggestions/{userId}")]
        public async Task<IActionResult> GetSuggestedMovies(int userId)
        {
            try
            {
                var result = await _movieService.GetSuggestedMovies(userId);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterMovies([FromQuery] MovieFilterRequestDto request)
        {
            try
            {
                var result = await _movieService.FilterMovies(request);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
