using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IPromoService
    {
        IEnumerable<PromoDto> GetAll();
        ApplyPromoResponseDto Apply(ApplyPromoRequestDto request);
    }
}
