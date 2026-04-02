using Microsoft.AspNetCore.Mvc;
using MovieRentalAPI.Exceptions;
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
        public async Task<IActionResult> AddInventory(InventoryRequestDto request)
        {
            try
            {
                var result = await _inventoryService.AddInventory(request);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (ConflictException ex) { return Conflict(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _inventoryService.GetAllInventory();
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _inventoryService.GetInventoryById(id);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetByMovie(int movieId)
        {
            try
            {
                var result = await _inventoryService.GetInventoryByMovie(movieId);
                return Ok(result);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, InventoryRequestDto request)
        {
            try
            {
                var result = await _inventoryService.UpdateInventory(id, request);
                return Ok(result);
            }
            catch (BadRequestException ex) { return BadRequest(ex.Message); }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _inventoryService.DeleteInventory(id);
                return Ok("Inventory deleted successfully");
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            try
            {
                await _inventoryService.ToggleAvailability(id);
                return Ok("Availability toggled successfully");
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}
