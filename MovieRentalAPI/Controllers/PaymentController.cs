using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponseDto>> MakePayment(
            MakePaymentRequestDto request)
        {
            var result = await _paymentService.MakePayment(request);
            return Ok(result);
        }

        [HttpGet("rental/{rentalId}")]
        public async Task<ActionResult<PaymentResponseDto>> GetByRental(int rentalId)
        {
            var result = await _paymentService.GetPaymentByRental(rentalId);

            if (result == null)
                return NotFound("Payment not found");

            return Ok(result);
        }
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<PaymentResponseDto>> GetByUser(int userId)
        {
            var result = await _paymentService.GetPaymentByUser(userId);

            if (result == null)
                return NotFound("Payment not found");

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetAll()
        {
            var result = await _paymentService.GetAllPayments();
            return Ok(result);
        }
    }
}
