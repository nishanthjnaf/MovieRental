using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet("{userId}/rented-movies")]
        public async Task<IActionResult> GetRentedMovies(int userId)
        {
            var result = await _userServices.GetAllRentedMovies(userId);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _userServices.GetUserById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // ✅ Get All Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = await _userServices.GetAllUsers();
            return Ok(users);
        }

        // ✅ Update User
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(
            int id,
            UpdateUserRequestDto request)
        {
            var updated = await _userServices.UpdateUser(id, request);

            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        // ✅ Delete User
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var success = await _userServices.DeleteUser(id);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}