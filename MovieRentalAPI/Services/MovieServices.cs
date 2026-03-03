using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Services
{
    public class MovieServices : IMovieServices
    {
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, Genre> _genreRepository;

        public MovieServices(
            IRepository<int, Movie> movieRepository,
            IRepository<int, Genre> genreRepository)
        {
            _movieRepository = movieRepository;
            _genreRepository = genreRepository;
        }

        public async Task<CreateMovieResponseDto> AddMovie(CreateMovieRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new BadRequestException("Movie title is required");

            var existingMovies = await _movieRepository.GetAll();

            if (existingMovies != null &&
                existingMovies.Any(m =>
                    m.Title.ToLower() == request.Title.ToLower()))
                throw new ConflictException("Movie already exists");

            var movie = new Movie
            {
                Title = request.Title,
                Description = request.Description,
                ReleaseYear = request.ReleaseYear,
                DurationMinutes = request.DurationMinutes,
                Language = request.Language,
                Genres = new List<Genre>(),
                RentalCount = 0
            };

            if (request.GenreIds != null && request.GenreIds.Any())
            {
                foreach (var genreId in request.GenreIds)
                {
                    var genre = await _genreRepository.Get(genreId);

                    if (genre == null)
                        throw new NotFoundException($"Genre {genreId} not found");

                    movie.Genres.Add(genre);
                }
            }

            var addedMovie = await _movieRepository.Add(movie);

            if (addedMovie == null)
                throw new Exception("Movie creation failed");

            return MapToResponseDto(addedMovie);
        }

        public async Task<CreateMovieResponseDto> GetMovieById(int id)
        {
            var movie = await _movieRepository.Get(id);

            if (movie == null)
                throw new NotFoundException("Movie not found");

            return MapToResponseDto(movie);
        }

        public async Task<IEnumerable<CreateMovieResponseDto>> GetAllMovies()
        {
            var movies = await _movieRepository.GetAll();

            if (movies == null || !movies.Any())
                throw new NotFoundException("No movies found");

            return movies.Select(MapToResponseDto);
        }

        public async Task<CreateMovieResponseDto> UpdateMovie(int id, CreateMovieRequestDto request)
        {
            var existingMovie = await _movieRepository.Get(id);

            if (existingMovie == null)
                throw new NotFoundException("Movie not found");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new BadRequestException("Movie title cannot be empty");

            existingMovie.Title = request.Title;
            existingMovie.Description = request.Description;
            existingMovie.ReleaseYear = request.ReleaseYear;
            existingMovie.DurationMinutes = request.DurationMinutes;
            existingMovie.Language = request.Language;

            var updatedMovie =
                await _movieRepository.Update(id, existingMovie);

            if (updatedMovie == null)
                throw new Exception("Movie update failed");

            return MapToResponseDto(updatedMovie);
        }

        public async Task<bool> DeleteMovie(int id)
        {
            var existingMovie = await _movieRepository.Get(id);

            if (existingMovie == null)
                throw new NotFoundException("Movie not found");

            var deleted = await _movieRepository.Delete(id);

            if (deleted == null)
                throw new Exception("Movie deletion failed");

            return true;
        }

        public async Task<PagedResultDto<CreateMovieResponseDto>> SearchMovies(
            MovieSearchRequestDto request)
        {
            if (request.PageNumber <= 0 || request.PageSize <= 0)
                throw new BadRequestException("Invalid pagination values");

            var movies = await _movieRepository.GetAll();

            if (movies == null || !movies.Any())
                throw new NotFoundException("No movies found");

            var filtered = movies
                .Where(m =>
                    string.IsNullOrEmpty(request.SearchTerm) ||
                    m.Title.Contains(request.SearchTerm,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!filtered.Any())
                throw new NotFoundException("No matching movies found");

            int totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResultDto<CreateMovieResponseDto>
            {
                Items = pagedItems.Select(MapToResponseDto).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(
                    (double)totalCount / request.PageSize)
            };
        }

        public async Task<IEnumerable<TopRentedMovieDto>> GetTopRentedMovies(int count)
        {
            if (count <= 0)
                throw new BadRequestException("Count must be greater than zero");

            var movies = await _movieRepository.GetAll();

            if (movies == null || !movies.Any())
                throw new NotFoundException("No movies found");

            return movies
                .OrderByDescending(m => m.RentalCount)
                .Take(count)
                .Select(m => new TopRentedMovieDto
                {
                    MovieId = m.Id,
                    Title = m.Title,
                    RentalCount = m.RentalCount,
                    ReleaseYear = m.ReleaseYear,
                    Language = m.Language
                })
                .ToList();
        }

        private CreateMovieResponseDto MapToResponseDto(Movie movie)
        {
            return new CreateMovieResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseYear = movie.ReleaseYear,
                DurationMinutes = movie.DurationMinutes,
                Language = movie.Language
            };
        }
    }
}