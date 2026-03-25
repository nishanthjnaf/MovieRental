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

        // GET api/Cart/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Movie)
                .Select(c => new
                {
                    c.Id,
                    c.MovieId,
                    c.RentalDays,
                    c.Movie!.Title,
                    c.Movie.PosterPath,
                    c.Movie.ReleaseYear,
                    c.Movie.Language,
                    c.Movie.Rating
                })
                .ToListAsync();

            return Ok(items);
        }

        // POST api/Cart/{userId}/add
        [HttpPost("{userId}/add")]
        public async Task<IActionResult> AddItem(int userId, [FromBody] CartAddRequest req)
        {
            var exists = await _context.CartItems
                .AnyAsync(c => c.UserId == userId && c.MovieId == req.MovieId);

            if (exists) return Ok(); // already in cart, no-op

            _context.CartItems.Add(new MovieRentalAPI.Models.CartItem
            {
                UserId = userId,
                MovieId = req.MovieId,
                RentalDays = req.RentalDays > 0 ? req.RentalDays : 7
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        // PATCH api/Cart/{userId}/days
        [HttpPatch("{userId}/days")]
        public async Task<IActionResult> UpdateDays(int userId, [FromBody] CartUpdateDaysRequest req)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.MovieId == req.MovieId);

            if (item == null) return NotFound();

            item.RentalDays = Math.Max(1, Math.Min(30, req.RentalDays));
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/Cart/{userId}/remove/{movieId}
        [HttpDelete("{userId}/remove/{movieId}")]
        public async Task<IActionResult> RemoveItem(int userId, int movieId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.MovieId == movieId);

            if (item == null) return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/Cart/{userId}/clear
        [HttpDelete("{userId}/clear")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var items = _context.CartItems.Where(c => c.UserId == userId);
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return Ok();
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
