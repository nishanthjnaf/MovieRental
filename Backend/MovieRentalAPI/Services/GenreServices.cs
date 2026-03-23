using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Services
{
    public class GenreService : IGenreService
    {
        private readonly IRepository<int, Genre> _genreRepository;
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, Review> _reviewRepository;

        public GenreService(
            IRepository<int, Genre> genreRepository,
            IRepository<int, Movie> movieRepository,
            IRepository<int, Review> reviewRepository)
        {
            _genreRepository = genreRepository;
            _movieRepository = movieRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<GenreResponseDto> AddGenre(GenreRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Genre name is required");

            var existingGenres = await _genreRepository.GetAll();

            if (existingGenres != null &&
                existingGenres.Any(g =>
                    g.Name.ToLower() == request.Name.ToLower()))
                throw new ConflictException("Genre already exists");

            var genre = new Genre
            {
                Name = request.Name,
                Description = request.Description
            };

            var added = await _genreRepository.Add(genre);

            if (added == null)
                throw new Exception("Genre creation failed");

            return MapToResponse(added);
        }

        public async Task<IEnumerable<GenreResponseDto>> GetAllGenres()
        {
            var genres = await _genreRepository.GetAll();

            if (genres == null || !genres.Any())
                throw new NotFoundException("No genres found");

            return genres.Select(MapToResponse);
        }

        public async Task<GenreResponseDto> GetGenreById(int id)
        {
            var genre = await _genreRepository.Get(id);

            if (genre == null)
                throw new NotFoundException("Genre not found");

            return MapToResponse(genre);
        }

        public async Task<GenreResponseDto> UpdateGenre(int id, GenreRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Genre name cannot be empty");

            var existing = await _genreRepository.Get(id);

            if (existing == null)
                throw new NotFoundException("Genre not found");

            existing.Name = request.Name;
            existing.Description = request.Description;

            var updated = await _genreRepository.Update(id, existing);

            if (updated == null)
                throw new Exception("Genre update failed");

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteGenre(int id)
        {
            var existing = await _genreRepository.Get(id);

            if (existing == null)
                throw new NotFoundException("Genre not found");

            var deleted = await _genreRepository.Delete(id);

            if (deleted == null)
                throw new Exception("Genre deletion failed");

            return true;
        }

        public async Task<bool> AssignGenreToMovie(int movieId, int genreId)
        {
            var movie = await _movieRepository.Get(movieId);

            if (movie == null)
                throw new NotFoundException("Movie not found");

            var genre = await _genreRepository.Get(genreId);

            if (genre == null)
                throw new NotFoundException("Genre not found");

            movie.Genres ??= new List<Genre>();

            if (movie.Genres.Any(g => g.Id == genreId))
                throw new ConflictException("Genre already assigned to this movie");

            movie.Genres.Add(genre);

            await _movieRepository.Update(movieId, movie);

            return true;
        }

        public async Task<IEnumerable<CreateMovieResponseDto>> GetMoviesByGenre(int genreId)
        {
            var genre = await _genreRepository
                .GetWithInclude(genreId, g => g.Movies);

            if (genre == null)
                throw new NotFoundException("Genre not found");

            if (genre.Movies == null || !genre.Movies.Any())
                throw new NotFoundException("No movies found for this genre");

            var allReviews = await _reviewRepository.GetAll();
            var avgByMovieId = allReviews?
                .GroupBy(r => r.MovieId)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating))
                ?? new Dictionary<int, double>();

            return genre.Movies.Select(m => new CreateMovieResponseDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                DurationMinutes = m.DurationMinutes,
                Language = m.Language,
                Rating = avgByMovieId.TryGetValue(m.Id, out var avg) ? avg : 0,
                Director = m.Director ?? string.Empty,
                Cast = SplitCsv(m.Cast),
                ContentRating = m.ContentRating ?? string.Empty,
                ContentAdvisory = SplitCsv(m.ContentAdvisory),
                PosterPath= m.PosterPath,
                TrailerUrl = m.TrailerUrl
                
            });
        }

        public async Task<IEnumerable<CreateMovieResponseDto>> GetMoviesByGenreName(string genreName)
        {
            if (string.IsNullOrWhiteSpace(genreName))
                throw new BadRequestException("Genre name is required");

            var allGenres = await _genreRepository.GetAll();
            var genre = allGenres?
                .FirstOrDefault(g => g.Name.Equals(genreName, StringComparison.OrdinalIgnoreCase));

            if (genre == null)
                throw new NotFoundException("Genre not found");

            return await GetMoviesByGenre(genre.Id);
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

        private static List<string> SplitCsv(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
    }
}