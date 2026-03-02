using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, RentalItem> _rentalItemRepository;

        public RentalController(IRentalService rentalService, IRepository<int,Movie> movieRepository,IRepository<int,RentalItem> rentalItemRepository)
        {
            _rentalService = rentalService;
            _movieRepository= movieRepository;
            _rentalItemRepository=rentalItemRepository;
        }


        [HttpPost]
        public async Task<ActionResult<RentalResponseDto>> CreateRental(
            CreateRentalRequestDto request)
        {
            var result = await _rentalService.CreateRental(request);
            
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<RentalResponseDto>>> GetByUser(int userId)
        {
            var result = await _rentalService.GetRentalsByUser(userId);
            return Ok(result);
        }

        [HttpGet("active/{userId}")]
        public async Task<ActionResult<IEnumerable<RentalItemResponseDto>>> GetActive(int userId)
        {
            var result = await _rentalService.GetActiveRentals(userId);
            return Ok(result);
        }

        [HttpPatch("end-item/{rentalItemId}")]
        public async Task<ActionResult> EndRentalItem(int rentalItemId)
        {
            var success = await _rentalService.EndRentalItem(rentalItemId);

            if (!success)
                return NotFound("Rental item not found");

            return Ok("Rental item ended successfully");
        }
    }
}
