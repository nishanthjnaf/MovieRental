using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromoController : ControllerBase
    {
        private readonly IPromoService _promoService;

        public PromoController(IPromoService promoService)
        {
            _promoService = promoService;
        }

        /// <summary>Returns all available promo codes.</summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_promoService.GetAll());
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        /// <summary>Validates a promo code against the current cart item count.</summary>
        [HttpPost("apply")]
        public IActionResult Apply(ApplyPromoRequestDto request)
        {
            try
            {
                if (request == null) return BadRequest("Request body is required");
                var result = _promoService.Apply(request);
                return result.IsValid ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
