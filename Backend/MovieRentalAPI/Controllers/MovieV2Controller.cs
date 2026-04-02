using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;
using MovieRentalModels;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/Movie")]
    [ApiExplorerSettings(GroupName = "v2")]
    public class MovieV2Controller : ControllerBase
    {
        private readonly IMovieServices _movieService;
        private readonly MovieRentalContext _context;

        public MovieV2Controller(IMovieServices movieService, MovieRentalContext context)
        {
            _movieService = movieService;
            _context = context;
        }

        /// <summary>
        /// V2: Returns enriched movie detail with AverageUserRating, TotalRentals, IsAvailable.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovie(int id)
        {
            try
            {
                // Reuse v1 service to get base movie data
                var base_movie = await _movieService.GetMovieById(id)
                    ?? throw new NotFoundException($"Movie with id {id} not found");

                // Enrich with live stats
                var avgRating = await _context.Reviews
                    .Where(r => r.MovieId == id)
                    .AverageAsync(r => (double?)r.Rating) ?? 0.0;

                var totalRentals = await _context.RentalItems
                    .CountAsync(ri => ri.MovieId == id);

                var isAvailable = await _context.Inventories
                    .AnyAsync(inv => inv.MovieId == id && inv.IsAvailable);

                var result = new MovieDetailV2ResponseDto
                {
                    Id = base_movie.Id,
                    Title = base_movie.Title,
                    Description = base_movie.Description,
                    ReleaseYear = base_movie.ReleaseYear,
                    DurationMinutes = base_movie.DurationMinutes,
                    Language = base_movie.Language,
                    Rating = base_movie.Rating,
                    Director = base_movie.Director,
                    Cast = base_movie.Cast,
                    ContentRating = base_movie.ContentRating,
                    Genres = base_movie.Genres,
                    ContentAdvisory = base_movie.ContentAdvisory,
                    PosterPath = base_movie.PosterPath,
                    TrailerUrl = base_movie.TrailerUrl,
                    AverageUserRating = Math.Round(avgRating, 1),
                    TotalRentals = totalRentals,
                    IsAvailable = isAvailable
                };

                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        /// <summary>
        /// V2: Returns all movies — same as V1 (no breaking change, just version-aware route).
        /// </summary>
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
    }
}
