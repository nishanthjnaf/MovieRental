using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IUserServices
    {
        public Task<CheckUserResponseDto> CheckUser(CheckUserRequestDto request);
        public Task<RegisterUserResponseDto> RegisterUser(RegisterUserRequestDto request);
        Task<IEnumerable<UserRentedMovieResponseDto>> GetAllRentedMovies(int userId);
        Task<UserResponseDto?> GetUserById(int id);

        Task<IEnumerable<UserResponseDto>> GetAllUsers();

        Task<UserResponseDto?> UpdateUser(int id, UpdateUserRequestDto request);

        Task<bool> DeleteUser(int id);


    }
}