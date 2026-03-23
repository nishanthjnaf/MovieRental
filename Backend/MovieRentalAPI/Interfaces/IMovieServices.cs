using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IMovieServices
    {
        Task<CreateMovieResponseDto> AddMovie(CreateMovieRequestDto request);
        Task<CreateMovieResponseDto?> GetMovieById(int id);
        Task<IEnumerable<CreateMovieResponseDto>> GetAllMovies();
        Task<CreateMovieResponseDto?> UpdateMovie(int id, CreateMovieRequestDto request);
        Task<bool> DeleteMovie(int id);
        Task<PagedResultDto<CreateMovieResponseDto>> SearchMovies(MovieSearchRequestDto request);
        Task<IEnumerable<TopRentedMovieDto>> GetTopRentedMovies(int count);
        Task<IEnumerable<CreateMovieResponseDto>> GetTopUserRatedMovies(int count);
    }
}
