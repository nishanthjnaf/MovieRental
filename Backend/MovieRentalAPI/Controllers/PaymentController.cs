using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

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
        //[Authorize(Roles = "Customer")]
        [HttpPost("refund/{rentalItemId}")]
        public async Task<IActionResult> ProcessRefund(int rentalItemId)
        {
            try
            {
                var result = await _paymentService.ProcessRefund(rentalItemId);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
        }

        //[Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> MakePayment(
            MakePaymentRequestDto request)
        {
            try
            {
                var result = await _paymentService.MakePayment(request);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Customer")]
        [HttpGet("item-refund/{rentalItemId}")]
        public async Task<IActionResult> GetItemRefund(int rentalItemId)
        {
            var result = await _paymentService.GetItemRefund(rentalItemId);
            if (result == null) return NotFound("No refund found for this item");
            return Ok(result);
        }

        //[Authorize(Roles = "Admin,Customer")]
        [HttpGet("rental/{rentalId}")]
        public async Task<IActionResult> GetByRental(int rentalId)
        {
            try
            {
                var result = await _paymentService.GetPaymentByRental(rentalId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin,Customer")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var result = await _paymentService.GetPaymentByUser(userId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }


        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _paymentService.GetAllPayments();
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}