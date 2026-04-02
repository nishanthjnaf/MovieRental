using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService _logService;

        public ActivityLogController(IActivityLogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] ActivityLogQueryDto query)
        {
            try
            {
                var result = await _logService.GetLogs(query);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
