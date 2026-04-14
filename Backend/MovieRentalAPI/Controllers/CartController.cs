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
                var movieItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Movie)
                    .ThenInclude(m => m!.Inventories)
                    .Select(c => new
                    {
                        c.Id, c.MovieId, c.RentalDays, c.IsRenewal,
                        c.Movie!.Title, c.Movie.PosterPath,
                        c.Movie.ReleaseYear, c.Movie.Language, c.Movie.Rating,
                        IsSeries = false,
                        SeriesId = (int?)null,
                        IsAvailable = c.Movie.Inventories.Any(i => i.IsAvailable)
                    })
                    .ToListAsync();

                var seriesItems = await _context.SeriesCartItems
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Series)
                    .Select(c => new
                    {
                        c.Id,
                        MovieId = (int?)null,
                        c.RentalDays,
                        IsRenewal = false,
                        c.Series!.Title,
                        c.Series.PosterPath,
                        ReleaseYear = (int?)null,
                        c.Series.Language,
                        Rating = (double?)null,
                        IsSeries = true,
                        SeriesId = (int?)c.SeriesId,
                        RentalPrice = c.Series.RentalPrice,
                        IsAvailable = c.Series.IsAvailable
                    })
                    .ToListAsync();

                var combined = movieItems.Cast<object>().Concat(seriesItems.Cast<object>());
                return Ok(combined);
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
                    RentalDays = req.RentalDays > 0 ? req.RentalDays : 7,
                    IsRenewal = req.IsRenewal
                });
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost("{userId}/add-series")]
        public async Task<IActionResult> AddSeriesItem(int userId, [FromBody] SeriesCartAddRequest req)
        {
            try
            {
                if (req == null || req.SeriesId <= 0) return BadRequest("Valid SeriesId is required");
                if (userId <= 0) return BadRequest("Valid UserId is required");

                var exists = await _context.SeriesCartItems
                    .AnyAsync(c => c.UserId == userId && c.SeriesId == req.SeriesId);
                if (exists) return Conflict("Series already exists in cart");

                _context.SeriesCartItems.Add(new MovieRentalAPI.Models.SeriesCartItem
                {
                    UserId = userId,
                    SeriesId = req.SeriesId,
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

        [HttpPatch("{userId}/series-days")]
        public async Task<IActionResult> UpdateSeriesDays(int userId, [FromBody] SeriesCartUpdateDaysRequest req)
        {
            try
            {
                var item = await _context.SeriesCartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.SeriesId == req.SeriesId);
                if (item == null) return NotFound("Series cart item not found");
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

        [HttpDelete("{userId}/remove-series/{seriesId}")]
        public async Task<IActionResult> RemoveSeriesItem(int userId, int seriesId)
        {
            try
            {
                var item = await _context.SeriesCartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.SeriesId == seriesId);
                if (item == null) return NotFound("Series cart item not found");
                _context.SeriesCartItems.Remove(item);
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
                var seriesItems = _context.SeriesCartItems.Where(c => c.UserId == userId);
                _context.SeriesCartItems.RemoveRange(seriesItems);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }

    public class CartAddRequest { public int MovieId { get; set; } public int RentalDays { get; set; } = 7; public bool IsRenewal { get; set; } = false; }
    public class CartUpdateDaysRequest { public int MovieId { get; set; } public int RentalDays { get; set; } }
    public class SeriesCartAddRequest { public int SeriesId { get; set; } public int RentalDays { get; set; } = 7; }
    public class SeriesCartUpdateDaysRequest { public int SeriesId { get; set; } public int RentalDays { get; set; } }
}
