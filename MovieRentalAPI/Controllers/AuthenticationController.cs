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
            catch (UnAuthorizedException )
            {
                return Unauthorized("Invalid username or password");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }

}