using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost]
        public async Task<ActionResult<InventoryResponseDto>> AddInventory(
            InventoryRequestDto request)
        {
            var result = await _inventoryService.AddInventory(request);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryResponseDto>>> GetAll()
        {
            var result = await _inventoryService.GetAllInventory();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryResponseDto>> GetById(int id)
        {
            var result = await _inventoryService.GetInventoryById(id);

            if (result == null)
                return NotFound("Inventory not found");

            return Ok(result);
        }

        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<InventoryResponseDto>> GetByMovie(int movieId)
        {
            var result = await _inventoryService.GetInventoryByMovie(movieId);

            if (result == null)
                return NotFound("Inventory not found for this movie");

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<InventoryResponseDto>> Update(
            int id,
            InventoryRequestDto request)
        {
            var result = await _inventoryService.UpdateInventory(id, request);

            if (result == null)
                return NotFound("Inventory not found");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _inventoryService.DeleteInventory(id);

            if (!success)
                return NotFound("Inventory not found");

            return Ok("Inventory deleted successfully");
        }

        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult> ToggleAvailability(int id)
        {
            var success = await _inventoryService.ToggleAvailability(id);

            if (!success)
                return NotFound("Inventory not found");

            return Ok("Availability toggled successfully");
        }
    }
}