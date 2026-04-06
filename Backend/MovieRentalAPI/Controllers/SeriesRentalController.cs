using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeriesRentalController : ControllerBase
    {
        private readonly ISeriesRentalService _service;

        public SeriesRentalController(ISeriesRentalService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRental(CreateSeriesRentalRequestDto request)
        {
            try { return Ok(await _service.CreateSeriesRental(request)); }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try { return Ok(await _service.GetSeriesRentalsByUser(userId)); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPatch("end/{id}")]
        public async Task<IActionResult> EndRental(int id)
        {
            try { await _service.EndSeriesRentalItem(id); return Ok("Ended"); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPatch("renew/{id}")]
        public async Task<IActionResult> RenewRental(int id, [FromBody] RenewRentalRequestDto request)
        {
            try { return Ok(await _service.RenewSeriesRentalItem(id, request)); }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
