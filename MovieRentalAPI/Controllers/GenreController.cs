using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        [HttpPost]
        public async Task<ActionResult> AddGenre(GenreRequestDto request)
        {
            var result = await _genreService.AddGenre(request);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllGenres()
        {
            var result = await _genreService.GetAllGenres();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetGenre(int id)
        {
            var result = await _genreService.GetGenreById(id);

            if (result == null)
                return NotFound("Genre not found");

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateGenre(int id, GenreRequestDto request)
        {
            var result = await _genreService.UpdateGenre(id, request);

            if (result == null)
                return NotFound("Genre not found");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGenre(int id)
        {
            var success = await _genreService.DeleteGenre(id);

            if (!success)
                return NotFound("Genre not found");

            return Ok("Genre deleted successfully");
        }

        [HttpPost("{genreId}/assign/{movieId}")]
        public async Task<ActionResult> AssignGenre(int genreId, int movieId)
        {
            var success = await _genreService.AssignGenreToMovie(movieId, genreId);

            if (!success)
                return NotFound("Movie or Genre not found");

            return Ok("Genre assigned to movie");
        }

        [HttpGet("{genreId}/movies")]
        public async Task<ActionResult> GetMoviesByGenre(int genreId)
        {
            var result = await _genreService.GetMoviesByGenre(genreId);
            return Ok(result);
        }
    }
}