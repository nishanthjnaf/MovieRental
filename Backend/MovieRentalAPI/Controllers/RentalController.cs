using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
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

        //[Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<ActionResult> CreateRental(CreateRentalRequestDto request)
        {
            try
            {
                var result = await _rentalService.CreateRental(request);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message); // 404
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message); // 409
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message); // 400
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        //[Authorize(Roles = "Admin,Customer")]
        [HttpGet("{rentalId}/items")]
        public async Task<ActionResult> GetRentalItems(int rentalId)
        {
            try
            {
                var result = await _rentalService.GetRentalItemsByRentalId(rentalId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin,Customer")]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult> GetByUser(int userId)
        {
            try
            {
                var result = await _rentalService.GetRentalsByUser(userId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> GetAllRentals()
        {
            try
            {
                var result = await _rentalService.GetAllRentals();
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin,Customer")]
        [HttpGet("active/{userId}")]
        public async Task<ActionResult> GetActive(int userId)
        {
            try
            {
                var result = await _rentalService.GetActiveRentals(userId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpPatch("end-item/{rentalItemId}")]
        public async Task<ActionResult> EndRentalItem(int rentalItemId)
        {
            try
            {
                await _rentalService.EndRentalItem(rentalItemId);
                return Ok("Rental item ended successfully");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin,Customer")]
        [HttpPatch("renew-item/{rentalItemId}")]
        public async Task<ActionResult> RenewRentalItem(
            int rentalItemId,
            [FromBody] RenewRentalRequestDto request)
        {
            try
            {
                var updated = await _rentalService
                    .RenewRentalItem(rentalItemId, request);

                return Ok(updated);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}