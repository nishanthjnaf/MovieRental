using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        [HttpPost]
        public async Task<IActionResult> AddSeries(SeriesRequestDto request)
        {
            try { return Ok(await _seriesService.AddSeries(request)); }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSeries(int id)
        {
            try { return Ok(await _seriesService.GetSeriesById(id)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSeries()
        {
            try { return Ok(await _seriesService.GetAllSeries()); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSeries(int id, SeriesRequestDto request)
        {
            try { return Ok(await _seriesService.UpdateSeries(id, request)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSeries(int id)
        {
            try { await _seriesService.DeleteSeries(id); return Ok("Series deleted"); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("new")]
        public async Task<IActionResult> GetNewSeries([FromQuery] int count = 10)
        {
            try { return Ok(await _seriesService.GetNewSeries(count)); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRated([FromQuery] int count = 10)
        {
            try { return Ok(await _seriesService.GetTopRatedSeries(count)); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("top-rented")]
        public async Task<IActionResult> GetTopRented([FromQuery] int count = 10)
        {
            try { return Ok(await _seriesService.GetTopRentedSeries(count)); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("suggestions/{userId}")]
        public async Task<IActionResult> GetSuggestions(int userId)
        {
            try { return Ok(await _seriesService.GetSuggestedSeries(userId)); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost("season")]
        public async Task<IActionResult> AddSeason(AddSeasonRequestDto request)
        {
            try { return Ok(await _seriesService.AddSeason(request)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost("episode")]
        public async Task<IActionResult> AddEpisode(AddEpisodeRequestDto request)
        {
            try { return Ok(await _seriesService.AddEpisode(request)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
