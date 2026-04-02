using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRentalModels;

namespace MovieRentalAPI.Controllers
{
    [ApiController]
    [Route("api/Cart")]
    public class CartController : ControllerBase
    {
        private readonly MovieRentalContext _context;

        public CartController(MovieRentalContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            try
            {
                var items = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Movie)
                    .Select(c => new
                    {
                        c.Id, c.MovieId, c.RentalDays,
                        c.Movie!.Title, c.Movie.PosterPath,
                        c.Movie.ReleaseYear, c.Movie.Language, c.Movie.Rating
                    })
                    .ToListAsync();
                return Ok(items);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost("{userId}/add")]
        public async Task<IActionResult> AddItem(int userId, [FromBody] CartAddRequest req)
        {
            try
            {
                if (req == null || req.MovieId <= 0) return BadRequest("Valid MovieId is required");
                if (userId <= 0) return BadRequest("Valid UserId is required");

                var exists = await _context.CartItems
                    .AnyAsync(c => c.UserId == userId && c.MovieId == req.MovieId);

                if (exists) return Conflict("Movie already exists in cart");

                _context.CartItems.Add(new MovieRentalAPI.Models.CartItem
                {
                    UserId = userId,
                    MovieId = req.MovieId,
                    RentalDays = req.RentalDays > 0 ? req.RentalDays : 7
                });
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPatch("{userId}/days")]
        public async Task<IActionResult> UpdateDays(int userId, [FromBody] CartUpdateDaysRequest req)
        {
            try
            {
                var item = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.MovieId == req.MovieId);

                if (item == null) return NotFound("Cart item not found");

                item.RentalDays = Math.Max(1, Math.Min(30, req.RentalDays));
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{userId}/remove/{movieId}")]
        public async Task<IActionResult> RemoveItem(int userId, int movieId)
        {
            try
            {
                var item = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.MovieId == movieId);

                if (item == null) return NotFound("Cart item not found");

                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpDelete("{userId}/clear")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            try
            {
                var items = _context.CartItems.Where(c => c.UserId == userId);
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }

    public class CartAddRequest
    {
        public int MovieId { get; set; }
        public int RentalDays { get; set; } = 7;
    }

    public class CartUpdateDaysRequest
    {
        public int MovieId { get; set; }
        public int RentalDays { get; set; }
    }
}
