using Microsoft.AspNetCore.Mvc;
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

        // GET api/Notification/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var result = await _svc.GetForUser(userId);
            return Ok(result);
        }

        // GET api/Notification/user/{userId}/unread-count
        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> UnreadCount(int userId)
        {
            var count = await _svc.GetUnreadCount(userId);
            return Ok(count);
        }

        // PATCH api/Notification/{id}/read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _svc.MarkRead(id);
            return Ok();
        }

        // PATCH api/Notification/user/{userId}/read-all
        [HttpPatch("user/{userId}/read-all")]
        public async Task<IActionResult> MarkAllRead(int userId)
        {
            await _svc.MarkAllRead(userId);
            return Ok();
        }

        // DELETE api/Notification/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.Delete(id);
            return Ok();
        }

        // POST api/Notification/admin/broadcast
        [HttpPost("admin/broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] SendBroadcastRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message is required");

            // Resolve sender username
            var sender = await _svc.GetSenderUsername(dto.SentByUserId);
            var result = await _svc.Broadcast("admin_message", dto.Title, dto.Message, dto.SentByUserId, sender);
            return Ok(result);
        }

        // GET api/Notification/admin/broadcasts
        [HttpGet("admin/broadcasts")]
        public async Task<IActionResult> GetBroadcasts()
        {
            var result = await _svc.GetAllBroadcasts();
            return Ok(result);
        }

        // DELETE api/Notification/admin/broadcasts/{id}
        [HttpDelete("admin/broadcasts/{id}")]
        public async Task<IActionResult> DeleteBroadcast(int id)
        {
            await _svc.DeleteBroadcast(id);
            return Ok();
        }

        // POST api/Notification/check-expiry
        [HttpPost("check-expiry")]
        public async Task<IActionResult> CheckExpiry()
        {
            await _svc.CheckExpiringRentals();
            return Ok();
        }

        // POST api/Notification/push  (generic single push)
        [HttpPost("push")]
        public async Task<IActionResult> Push([FromBody] PushNotificationDto dto)
        {
            await _svc.Push(dto.UserId, dto.Type, dto.Title, dto.Message, dto.RelatedId);
            return Ok();
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
