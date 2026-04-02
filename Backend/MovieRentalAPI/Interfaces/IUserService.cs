using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IUserServices
    {
        Task<CheckUserResponseDto> CheckUser(CheckUserRequestDto request);
        Task<RegisterUserResponseDto> RegisterUser(RegisterUserRequestDto request);
        Task<IEnumerable<UserRentedMovieResponseDto>> GetAllRentedMovies(int userId);
        Task<UserResponseDto> GetUserById(int id);
        Task<UserResponseDto> GetUserByUsername(string username);
        Task<IEnumerable<UserResponseDto>> GetAllUsers();
        Task<UserResponseDto> UpdateUser(int id, UpdateUserRequestDto request);
        Task<bool> DeleteUser(int id);
        Task<bool> ResetPassword(int id, ResetPasswordRequestDto request);
        Task<UserPreferenceResponseDto> SavePreferences(int userId, SavePreferenceRequestDto request);
        Task<UserPreferenceResponseDto> GetPreferences(int userId);
    }
}
