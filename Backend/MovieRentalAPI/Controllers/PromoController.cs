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
        public IActionResult GetAll() => Ok(_promoService.GetAll());

        /// <summary>Validates a promo code against the current cart item count.</summary>
        [HttpPost("apply")]
        public IActionResult Apply(ApplyPromoRequestDto request)
        {
            var result = _promoService.Apply(request);
            return result.IsValid ? Ok(result) : BadRequest(result);
        }
    }
}
