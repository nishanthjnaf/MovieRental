using Microsoft.AspNetCore.Mvc;
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

        /// <summary>
        /// Returns paginated activity logs with optional filters and sort.
        /// </summary>
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] ActivityLogQueryDto query)
        {
            var result = await _logService.GetLogs(query);
            return Ok(result);
        }
    }
}
