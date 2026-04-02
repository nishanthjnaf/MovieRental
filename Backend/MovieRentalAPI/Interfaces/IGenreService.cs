using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface IGenreService
    {
        Task<GenreResponseDto> AddGenre(GenreRequestDto request);
        Task<IEnumerable<GenreResponseDto>> GetAllGenres();
        Task<GenreResponseDto> GetGenreById(int id);
        Task<GenreResponseDto> UpdateGenre(int id, GenreRequestDto request);
        Task<bool> DeleteGenre(int id);
        Task<bool> AssignGenreToMovie(int movieId, int genreId);
        Task<IEnumerable<CreateMovieResponseDto>> GetMoviesByGenre(int genreId);
        Task<IEnumerable<CreateMovieResponseDto>> GetMoviesByGenreName(string genreName);
    }
}
