using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface ISeriesService
    {
        Task<SeriesResponseDto> AddSeries(SeriesRequestDto request);
        Task<SeriesResponseDto> GetSeriesById(int id);
        Task<IEnumerable<SeriesResponseDto>> GetAllSeries();
        Task<SeriesResponseDto> UpdateSeries(int id, SeriesRequestDto request);
        Task<bool> DeleteSeries(int id);
        Task<IEnumerable<SeriesResponseDto>> GetNewSeries(int count);
        Task<IEnumerable<SeriesResponseDto>> GetTopRatedSeries(int count);
        Task<IEnumerable<SeriesResponseDto>> GetTopRentedSeries(int count);
        Task<IEnumerable<SeriesResponseDto>> GetSuggestedSeries(int userId);
    }
}
