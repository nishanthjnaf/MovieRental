using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

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

        //[Authorize(Roles = "Admin,CUstomer")]
        [HttpGet("{userId}/rented-movies")]
        public async Task<IActionResult> GetRentedMovies(int userId)
        {
            try
            {
                var result = await _userServices.GetAllRentedMovies(userId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userServices.GetUserById(id);
                return Ok(user);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin,Customer")]
        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userServices.GetUserByUsername(username);
                return Ok(user);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userServices.GetAllUsers();
                return Ok(users);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Customer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequestDto request)
        {
            try
            {
                var updated = await _userServices.UpdateUser(id, request);
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
        }

        //[Authorize(Roles = "Customer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userServices.DeleteUser(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Customer")]
        [HttpPatch("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(
            int id,
            [FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                await _userServices.ResetPassword(id, request);
                return Ok("Password reset successfully");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/preferences")]
        public async Task<IActionResult> SavePreferences(int id, [FromBody] SavePreferenceRequestDto request)
        {
            try
            {
                var result = await _userServices.SavePreferences(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/preferences")]
        public async Task<IActionResult> GetPreferences(int id)
        {
            try
            {
                var result = await _userServices.GetPreferences(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}