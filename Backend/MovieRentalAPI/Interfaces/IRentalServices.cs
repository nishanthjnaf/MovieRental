using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IRentalService
    {
        Task<RentalResponseDto> CreateRental(CreateRentalRequestDto request);

        Task<IEnumerable<RentalResponseDto>> GetRentalsByUser(int userId);

        Task<IEnumerable<RentalItemResponseDto>> GetActiveRentals(int userId);

        Task<bool> EndRentalItem(int rentalItemId);
        Task<RentalItemResponseDto> RenewRentalItem(int rentalItemId, RenewRentalRequestDto request);
        Task<IEnumerable<RentalItemResponseDto>> GetRentalItemsByRentalId(int rentalId);
        Task<IEnumerable<RentalResponseDto>> GetAllRentals();
    }
}