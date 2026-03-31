using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Helpers;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalModels;

namespace MovieRentalAPI.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly MovieRentalContext _context;

        public ActivityLogService(MovieRentalContext context)
        {
            _context = context;
        }

        public async Task Log(int userId, string userName, string role,
                              string entity, string action, string details,
                              string status = "Success")
        {
            var entry = new ActivityLog
            {
                UserId = userId,
                UserName = userName,
                Role = role,
                Entity = entity,
                Action = action,
                Details = details,
                Status = status,
                PerformedAt = IstDateTime.Now
            };
            _context.ActivityLogs.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedActivityLogDto> GetLogs(ActivityLogQueryDto q)
        {
            var query = _context.ActivityLogs.AsQueryable();

            if (q.UserId.HasValue)
                query = query.Where(l => l.UserId == q.UserId.Value);

            if (!string.IsNullOrWhiteSpace(q.Role))
                query = query.Where(l => l.Role == q.Role);

            if (!string.IsNullOrWhiteSpace(q.Entity))
                query = query.Where(l => l.Entity == q.Entity);

            if (!string.IsNullOrWhiteSpace(q.Action))
                query = query.Where(l => l.Action.Contains(q.Action));

            if (!string.IsNullOrWhiteSpace(q.Status))
                query = query.Where(l => l.Status == q.Status);

            if (q.From.HasValue)
                query = query.Where(l => l.PerformedAt >= q.From.Value);

            if (q.To.HasValue)
                query = query.Where(l => l.PerformedAt <= q.To.Value.AddDays(1).AddSeconds(-1));

            var total = await query.CountAsync();

            query = q.SortOrder == "asc"
                ? query.OrderBy(l => l.PerformedAt)
                : query.OrderByDescending(l => l.PerformedAt);

            var page = Math.Max(1, q.Page);
            var size = Math.Clamp(q.PageSize, 1, 200);

            var items = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(l => new ActivityLogDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.UserName,
                    Role = l.Role,
                    Entity = l.Entity,
                    Action = l.Action,
                    Details = l.Details,
                    Status = l.Status,
                    PerformedAt = l.PerformedAt
                })
                .ToListAsync();

            return new PagedActivityLogDto
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = size
            };
        }
    }
}
