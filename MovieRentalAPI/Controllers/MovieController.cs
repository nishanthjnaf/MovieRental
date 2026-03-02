using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;

    namespace MovieRentalAPI.Controllers

    {
        [ApiController]
        [Route("api/[controller]")]
        public class MovieController : ControllerBase
        {
            private readonly IMovieServices _movieService;
            public MovieController(IMovieServices movieService)
            {
                _movieService = movieService;
            }


            [HttpPost]
            public async Task<ActionResult<CreateMovieResponseDto>> AddMovie(
                CreateMovieRequestDto request)
            {
                var result = await _movieService.AddMovie(request);
                return Ok(result);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<CreateMovieResponseDto>> GetMovie(int id)
            {
                var result = await _movieService.GetMovieById(id);
                if (result == null)
                    return NotFound("Movie not found");
                return Ok(result);
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<CreateMovieResponseDto>>> GetAllMovies()
            {
                var result = await _movieService.GetAllMovies();
                return Ok(result);
            }
            [HttpGet("search")]
            public async Task<IActionResult> SearchMovies(
                   [FromQuery] MovieSearchRequestDto request)
            {
                var result = await _movieService.SearchMovies(request);
                return Ok(result);
            }

            [HttpPut("{id}")]
            public async Task<ActionResult<CreateMovieResponseDto>> UpdateMovie(
                int id, CreateMovieRequestDto request)
            {
                var result = await _movieService.UpdateMovie(id, request);
                if (result == null)
                    return NotFound("Movie not found");
                return Ok(result);
            }

            [HttpDelete("{id}")]
            public async Task<ActionResult> DeleteMovie(int id)
            {
                var success = await _movieService.DeleteMovie(id);
                if (!success)
                    return NotFound("Movie not found");
                return Ok("Movie deleted successfully");
            }
        [HttpGet("top-rented")]
        public async Task<IActionResult> GetTopRentedMovies([FromQuery] int count = 5)
        {
            var result = await _movieService.GetTopRentedMovies(count);

            return Ok(result);
        }
    }
    }

 