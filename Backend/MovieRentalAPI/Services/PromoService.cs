using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Services
{
    public class PromoService : IPromoService
    {
        private static readonly List<PromoDto> _promos = new()
        {
            new PromoDto { Code = "DOUBLE5",  Label = "5% OFF",  Description = "Rent 2 movies and save 5%",  MinItems = 2, DiscountPct = 5  },
            new PromoDto { Code = "TRIPLE10", Label = "10% OFF", Description = "Rent 3 movies and save 10%", MinItems = 3, DiscountPct = 10 }
        };

        public IEnumerable<PromoDto> GetAll() => _promos;

        public ApplyPromoResponseDto Apply(ApplyPromoRequestDto request)
        {
            var promo = _promos.FirstOrDefault(p =>
                p.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase));

            if (promo == null)
                return new ApplyPromoResponseDto { IsValid = false, Message = "Invalid promo code" };

            if (request.ItemCount < promo.MinItems)
                return new ApplyPromoResponseDto
                {
                    IsValid = false,
                    Code = promo.Code,
                    Message = $"Add {promo.MinItems - request.ItemCount} more movie(s) to use this offer"
                };

            return new ApplyPromoResponseDto
            {
                IsValid = true,
                Code = promo.Code,
                DiscountPct = promo.DiscountPct,
                Message = $"{promo.Label} applied!"
            };
        }
    }
}
