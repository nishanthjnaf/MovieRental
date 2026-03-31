using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IActivityLogService
    {
        Task Log(int userId, string userName, string role,
                 string entity, string action, string details,
                 string status = "Success");

        Task<PagedActivityLogDto> GetLogs(ActivityLogQueryDto query);
    }
}
