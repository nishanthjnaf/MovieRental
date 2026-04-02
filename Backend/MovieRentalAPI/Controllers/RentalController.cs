using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpPost]
        public async Task<ActionResult> CreateRental(CreateRentalRequestDto request)
        {
            try
            {
                var result = await _rentalService.CreateRental(request);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("{rentalId}/items")]
        public async Task<ActionResult> GetRentalItems(int rentalId)
        {
            try
            {
                var result = await _rentalService.GetRentalItemsByRentalId(rentalId);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult> GetByUser(int userId)
        {
            try
            {
                var result = await _rentalService.GetRentalsByUser(userId);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet]
        public async Task<ActionResult> GetAllRentals()
        {
            try
            {
                var result = await _rentalService.GetAllRentals();
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("active/{userId}")]
        public async Task<ActionResult> GetActive(int userId)
        {
            try
            {
                var result = await _rentalService.GetActiveRentals(userId);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPatch("end-item/{rentalItemId}")]
        public async Task<ActionResult> EndRentalItem(int rentalItemId)
        {
            try
            {
                await _rentalService.EndRentalItem(rentalItemId);
                return Ok("Rental item ended successfully");
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPatch("renew-item/{rentalItemId}")]
        public async Task<ActionResult> RenewRentalItem(int rentalItemId, [FromBody] RenewRentalRequestDto request)
        {
            try
            {
                var updated = await _rentalService.RenewRentalItem(rentalItemId, request);
                return Ok(updated);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
