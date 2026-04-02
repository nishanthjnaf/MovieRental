using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _svc;

        public NotificationController(NotificationService svc)
        {
            _svc = svc;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            try
            {
                if (userId <= 0) return BadRequest("Valid UserId is required");
                var result = await _svc.GetForUser(userId);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> UnreadCount(int userId)
        {
            try
            {
                if (userId <= 0) return BadRequest("Valid UserId is required");
                var count = await _svc.GetUnreadCount(userId);
                return Ok(count);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Valid notification Id is required");
                await _svc.MarkRead(id);
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPatch("user/{userId}/read-all")]
        public async Task<IActionResult> MarkAllRead(int userId)
        {
            try
            {
                if (userId <= 0) return BadRequest("Valid UserId is required");
                await _svc.MarkAllRead(userId);
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Valid notification Id is required");
                await _svc.Delete(id);
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost("admin/broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] SendBroadcastRequestDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Request body is required");
                if (string.IsNullOrWhiteSpace(dto.Message))
                    return BadRequest("Message is required");

                var sender = await _svc.GetSenderUsername(dto.SentByUserId);
                var result = await _svc.Broadcast("admin_message", dto.Title, dto.Message, dto.SentByUserId, sender);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("admin/broadcasts")]
        public async Task<IActionResult> GetBroadcasts()
        {
            try
            {
                var result = await _svc.GetAllBroadcasts();
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("admin/broadcasts/{id}")]
        public async Task<IActionResult> DeleteBroadcast(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Valid broadcast Id is required");
                await _svc.DeleteBroadcast(id);
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("check-expiry")]
        public async Task<IActionResult> CheckExpiry()
        {
            try
            {
                await _svc.CheckExpiringRentals();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("check-expired")]
        public async Task<IActionResult> CheckExpired()
        {
            try
            {
                await _svc.CheckExpiredRentals();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost("push")]
        public async Task<IActionResult> Push([FromBody] PushNotificationDto dto)
        {
            try
            {
                if (dto.UserId <= 0)
                    return BadRequest("Valid UserId is required");
                await _svc.Push(dto.UserId, dto.Type, dto.Title, dto.Message, dto.RelatedId);
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }

    public class PushNotificationDto
    {
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
    }
}
