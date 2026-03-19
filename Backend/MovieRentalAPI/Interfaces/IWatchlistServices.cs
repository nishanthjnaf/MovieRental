using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IWatchlistService
    {
        Task<WatchlistResponseDto> AddToWatchlist(WatchlistRequestDto request);

        Task<IEnumerable<WatchlistResponseDto>> GetUserWatchlist(int userId);

        Task<bool> RemoveFromWatchlist(int id);
    }
}
