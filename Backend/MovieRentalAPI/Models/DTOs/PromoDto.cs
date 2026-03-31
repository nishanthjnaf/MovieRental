namespace MovieRentalAPI.Models.DTOs
{
    public class PromoDto
    {
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MinItems { get; set; }
        public double DiscountPct { get; set; }
    }

    public class ApplyPromoRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }

    public class ApplyPromoResponseDto
    {
        public bool IsValid { get; set; }
        public string Code { get; set; } = string.Empty;
        public double DiscountPct { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
