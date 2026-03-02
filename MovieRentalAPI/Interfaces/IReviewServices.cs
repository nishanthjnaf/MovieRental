using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> AddReview(ReviewRequestDto request);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByMovie(int movieId);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByUser(int userId);
        Task<ReviewResponseDto?> UpdateReview(int id, ReviewRequestDto request);
        Task<bool> DeleteReview(int id);
    }
}
