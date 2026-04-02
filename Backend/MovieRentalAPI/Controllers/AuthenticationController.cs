using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserServices _userService;

        public AuthenticationController(IUserServices userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<CheckUserResponseDto>> Login(CheckUserRequestDto userRequestDto)
        {
            try
            {
                var result = await _userService.CheckUser(userRequestDto);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (UnAuthorizedException ex) { return Unauthorized(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<RegisterUserResponseDto>> Register(RegisterUserRequestDto registerRequestDto)
        {
            try
            {
                var result = await _userService.RegisterUser(registerRequestDto);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
