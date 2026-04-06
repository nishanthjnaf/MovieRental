using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface ISeriesWatchlistService
    {
        Task<SeriesWatchlistResponseDto> AddToWatchlist(SeriesWatchlistRequestDto request);
        Task<IEnumerable<SeriesWatchlistResponseDto>> GetUserWatchlist(int userId);
        Task<bool> RemoveFromWatchlist(int id);
    }
}
