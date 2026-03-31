using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Helpers;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalModels;

namespace MovieRentalAPI.Services
{
    public class NotificationService
    {
        private readonly MovieRentalContext _context;

        public NotificationService(MovieRentalContext context)
        {
            _context = context;
        }

        // ── Push a single notification ──────────────────────────────────────
        public async Task Push(int userId, string type, string title, string message, int? relatedId = null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = IstDateTime.Now,
                RelatedId = relatedId
            });
            await _context.SaveChangesAsync();
        }

        // ── Broadcast to all customers ──────────────────────────────────────
        public async Task<BroadcastMessageDto> Broadcast(string type, string title, string message, int sentByUserId = 0, string sentByUsername = "Admin", int? relatedId = null)
        {
            var now = IstDateTime.Now;

            var customerIds = await _context.Set<User>()
                .Where(u => u.Role == "Customer")
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var uid in customerIds)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = uid,
                    Type = type,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    CreatedAt = now,
                    RelatedId = relatedId
                });
            }

            // Save broadcast record for admin history
            var record = new BroadcastMessage
            {
                SentByUserId = sentByUserId,
                SentByUsername = sentByUsername,
                Title = title,
                Message = message,
                SentAt = now
            };
            _context.BroadcastMessages.Add(record);
            await _context.SaveChangesAsync();

            return new BroadcastMessageDto
            {
                Id = record.Id,
                SentByUserId = record.SentByUserId,
                SentByUsername = record.SentByUsername,
                Title = record.Title,
                Message = record.Message,
                SentAt = record.SentAt
            };
        }

        // ── Get all broadcast history ────────────────────────────────────────
        public async Task<IEnumerable<BroadcastMessageDto>> GetAllBroadcasts()
        {
            return await _context.BroadcastMessages
                .OrderByDescending(b => b.SentAt)
                .Select(b => new BroadcastMessageDto
                {
                    Id = b.Id,
                    SentByUserId = b.SentByUserId,
                    SentByUsername = b.SentByUsername,
                    Title = b.Title,
                    Message = b.Message,
                    SentAt = b.SentAt
                })
                .ToListAsync();
        }

        // ── Delete a broadcast record ────────────────────────────────────────
        public async Task DeleteBroadcast(int id)
        {
            var b = await _context.BroadcastMessages.FindAsync(id);
            if (b != null) { _context.BroadcastMessages.Remove(b); await _context.SaveChangesAsync(); }
        }

        // ── Resolve sender username ──────────────────────────────────────────
        public async Task<string> GetSenderUsername(int userId)
        {
            var user = await _context.Set<User>().FindAsync(userId);
            return user?.Username ?? "Admin";
        }

        // ── Notify users whose preferences match a new movie ────────────────
        public async Task NotifyNewMovie(int movieId, string movieTitle, IEnumerable<string> genreNames, string language)
        {
            var prefs = await _context.UserPreferences.Where(p => p.IsSet).ToListAsync();
            var genreNamesLower = genreNames.Select(g => g.ToLower()).ToList();

            foreach (var pref in prefs)
            {
                var prefGenres = (pref.PreferredGenres ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower()).ToList();

                var prefLangs = (pref.PreferredLanguages ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower()).ToList();

                bool genreMatch = prefGenres.Count == 0 || prefGenres.Any(g => genreNamesLower.Contains(g));
                bool langMatch = prefLangs.Count == 0 || prefLangs.Contains(language.ToLower());

                if (genreMatch && langMatch)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = pref.UserId,
                        Type = "new_movie",
                        Title = "New Movie Added",
                        Message = $"\"{movieTitle}\" has been added and matches your preferences.",
                        IsRead = false,
                        CreatedAt = IstDateTime.Now,
                        RelatedId = movieId
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        // ── Check expiring rentals (called on demand or scheduled) ──────────
        public async Task CheckExpiringRentals()
        {
            var cutoff = IstDateTime.Now.AddHours(24);
            var now = IstDateTime.Now;

            var expiringItems = await _context.Set<RentalItem>()
                .Where(i => i.IsActive && i.EndDate > now && i.EndDate <= cutoff)
                .ToListAsync();

            foreach (var item in expiringItems)
            {
                // Avoid duplicate notifications within the last 24 hours
                var alreadyNotified = await _context.Notifications.AnyAsync(n =>
                    n.Type == "expiry" && n.RelatedId == item.MovieId &&
                    n.UserId == _context.Set<Rental>().Where(r => r.Id == item.RentalId).Select(r => r.UserId).FirstOrDefault() &&
                    n.CreatedAt >= now.AddHours(-24));

                if (alreadyNotified) continue;

                var rental = await _context.Set<Rental>().FindAsync(item.RentalId);
                if (rental == null) continue;

                var movie = await _context.Set<Movie>().FindAsync(item.MovieId);
                var hoursLeft = (int)(item.EndDate - now).TotalHours;

                _context.Notifications.Add(new Notification
                {
                    UserId = rental.UserId,
                    Type = "expiry",
                    Title = "Rental Expiring Soon",
                    Message = $"Your rental of \"{movie?.Title ?? "a movie"}\" expires in {hoursLeft} hour(s). Renew now to keep watching.",
                    IsRead = false,
                    CreatedAt = now,
                    RelatedId = item.MovieId  // navigate to movie detail page
                });
            }
            await _context.SaveChangesAsync();
        }

        // ── Check just-expired rentals ───────────────────────────────────────
        public async Task CheckExpiredRentals()
        {
            var now = IstDateTime.Now;
            // Items that expired in the last 30 minutes (window matches scheduler interval)
            var windowStart = now.AddMinutes(-30);

            var justExpiredItems = await _context.Set<RentalItem>()
                .Where(i => i.IsActive && i.EndDate >= windowStart && i.EndDate <= now)
                .ToListAsync();

            foreach (var item in justExpiredItems)
            {
                var rental = await _context.Set<Rental>().FindAsync(item.RentalId);
                if (rental == null) continue;

                // Avoid duplicate expired notifications for the same item
                var alreadyNotified = await _context.Notifications.AnyAsync(n =>
                    n.Type == "expired" && n.RelatedId == item.Id &&
                    n.UserId == rental.UserId);

                if (alreadyNotified) continue;

                var movie = await _context.Set<Movie>().FindAsync(item.MovieId);

                _context.Notifications.Add(new Notification
                {
                    UserId = rental.UserId,
                    Type = "expired",
                    Title = "Rental Expired",
                    Message = $"Your rental of \"{movie?.Title ?? "a movie"}\" has expired. Renew to continue watching.",
                    IsRead = false,
                    CreatedAt = now,
                    RelatedId = item.Id  // navigate to Expired section in My Rentals
                });

                // Deactivate the item
                item.IsActive = false;
                _context.Set<RentalItem>().Update(item);
            }
            await _context.SaveChangesAsync();
        }

        // ── Get notifications for a user ────────────────────────────────────
        public async Task<IEnumerable<NotificationDto>> GetForUser(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    RelatedId = n.RelatedId
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCount(int userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkRead(int notificationId)
        {
            var n = await _context.Notifications.FindAsync(notificationId);
            if (n != null) { n.IsRead = true; await _context.SaveChangesAsync(); }
        }

        public async Task MarkAllRead(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            unread.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int notificationId)
        {
            var n = await _context.Notifications.FindAsync(notificationId);
            if (n != null) { _context.Notifications.Remove(n); await _context.SaveChangesAsync(); }
        }
    }
}
