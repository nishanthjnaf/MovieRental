using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface ISeriesRentalService
    {
        Task<RentalResponseDto> CreateSeriesRental(CreateSeriesRentalRequestDto request);
        Task<IEnumerable<SeriesRentalItemResponseDto>> GetSeriesRentalsByUser(int userId);
        Task<bool> EndSeriesRentalItem(int seriesRentalItemId);
        Task<SeriesRentalItemResponseDto> RenewSeriesRentalItem(int seriesRentalItemId, RenewRentalRequestDto request);
    }
}
