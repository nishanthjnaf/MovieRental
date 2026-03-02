using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Services
{
    public class GenreService : IGenreService
    {
        private readonly IRepository<int, Genre> _genreRepository;
        private readonly IRepository<int, Movie> _movieRepository;

        public GenreService(
            IRepository<int, Genre> genreRepository,
            IRepository<int, Movie> movieRepository)
        {
            _genreRepository = genreRepository;
            _movieRepository = movieRepository;
        }

        public async Task<GenreResponseDto> AddGenre(GenreRequestDto request)
        {
            var genre = new Genre
            {
                Name = request.Name,
                Description = request.Description
            };

            var added = await _genreRepository.Add(genre);

            return MapToResponse(added);
        }

        public async Task<IEnumerable<GenreResponseDto>> GetAllGenres()
        {
            var genres = await _genreRepository.GetAll();

            if (genres == null)
                return new List<GenreResponseDto>();

            return genres.Select(MapToResponse);
        }

        public async Task<GenreResponseDto?> GetGenreById(int id)
        {
            var genre = await _genreRepository.Get(id);
            return genre == null ? null : MapToResponse(genre);
        }

        public async Task<GenreResponseDto?> UpdateGenre(int id, GenreRequestDto request)
        {
            var existing = await _genreRepository.Get(id);

            if (existing == null)
                return null;

            existing.Name = request.Name;
            existing.Description = request.Description;

            var updated = await _genreRepository.Update(id, existing);

            return updated == null ? null : MapToResponse(updated);
        }

        public async Task<bool> DeleteGenre(int id)
        {
            var deleted = await _genreRepository.Delete(id);
            return deleted != null;
        }

        public async Task<bool> AssignGenreToMovie(int movieId, int genreId)
        {
            var movie = await _movieRepository.Get(movieId);
            var genre = await _genreRepository.Get(genreId);

            if (movie == null || genre == null)
                return false;

            movie.Genres ??= new List<Genre>();
            genre.Movies ??= new List<Movie>();

            if (!movie.Genres.Contains(genre))
            {
                movie.Genres.Add(genre);
                genre.Movies.Add(movie);
            }

            await _movieRepository.Update(movieId, movie);
            await _genreRepository.Update(genreId, genre);

            return true;
        }

        public async Task<IEnumerable<CreateMovieResponseDto>> GetMoviesByGenre(int genreId)
        {
            var genre = await _genreRepository
                .GetWithInclude(genreId, g => g.Movies);

            if (genre?.Movies == null)
                return new List<CreateMovieResponseDto>();

            return genre.Movies.Select(m => new CreateMovieResponseDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                DurationMinutes = m.DurationMinutes,
                Language = m.Language
            });
        }

        private GenreResponseDto MapToResponse(Genre genre)
        {
            return new GenreResponseDto
            {
                Id = genre.Id,
                Name = genre.Name,
                Description = genre.Description
            };
        }
    }
}