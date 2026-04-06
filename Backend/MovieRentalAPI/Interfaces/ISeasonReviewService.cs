using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface ISeasonReviewService
    {
        Task<SeasonReviewResponseDto> AddReview(SeasonReviewRequestDto request);
        Task<IEnumerable<SeasonReviewResponseDto>> GetReviewsBySeason(int seasonId);
        Task<IEnumerable<SeasonReviewResponseDto>> GetReviewsByUser(int userId);
        Task<bool> DeleteReview(int id);
    }
}
