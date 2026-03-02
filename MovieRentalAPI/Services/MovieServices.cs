using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;


namespace MovieRentalAPI.Services
{
    public class MovieServices : IMovieServices
    {
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, Genre> _genreRepository;

        public MovieServices(IRepository<int, Movie> movieRepository, IRepository<int, Genre> genreRepository)
        {
            _movieRepository = movieRepository;
            _genreRepository = genreRepository;

        }
        public async Task<CreateMovieResponseDto> AddMovie(CreateMovieRequestDto request)
        {
            var movie = new Movie
            {

                Title = request.Title,
                Description = request.Description,
                ReleaseYear = request.ReleaseYear,
                DurationMinutes = request.DurationMinutes,
                Language = request.Language,
                Genres = new List<Genre>()

            };
            if (request.GenreIds != null && request.GenreIds.Any())
            {
                foreach (var genreId in request.GenreIds)
                {
                    var genre = await _genreRepository.Get(genreId);
                    if (genre != null)
                        movie.Genres.Add(genre);
                }
            }
            var addedMovie = await _movieRepository.Add(movie);
            return MapToResponseDto(addedMovie);
        }

            
        public async Task<CreateMovieResponseDto?> GetMovieById(int id)
        {
            var movie = await _movieRepository.Get(id);
            return movie == null ? null : MapToResponseDto(movie);
        }
        public async Task<IEnumerable<CreateMovieResponseDto>> GetAllMovies()
        {
            var movies = await _movieRepository.GetAll();
            if (movies == null)
                return new List<CreateMovieResponseDto>();
            return movies.Select(MapToResponseDto);
        }
        public async Task<CreateMovieResponseDto?> UpdateMovie(int id, CreateMovieRequestDto request)
        {
            var existingMovie = await _movieRepository.Get(id);
            if (existingMovie == null)
                return null;
            existingMovie.Title = request.Title;
            existingMovie.Description = request.Description;
            existingMovie.ReleaseYear = request.ReleaseYear;
            existingMovie.DurationMinutes = request.DurationMinutes;
            existingMovie.Language = request.Language;
            var updatedMovie = await _movieRepository.Update(id, existingMovie);
            return updatedMovie == null ? null : MapToResponseDto(updatedMovie);
        }
        public async Task<bool> DeleteMovie(int id)
        {
            var deleted = await _movieRepository.Delete(id);
            return deleted != null;
        }
        public async Task<PagedResultDto<CreateMovieResponseDto>> SearchMovies(MovieSearchRequestDto request)
        {
            var movies = await _movieRepository.GetAll();

            if (movies == null)
                return new PagedResultDto<CreateMovieResponseDto>();

            var filtered = movies
                .Where(m => string.IsNullOrEmpty(request.SearchTerm) ||
                            m.Title.Contains(request.SearchTerm,
                                StringComparison.OrdinalIgnoreCase))
                .ToList();

            int totalCount = filtered.Count;

            var pagedItems = filtered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var result = pagedItems.Select(MapToResponseDto).ToList();

            return new PagedResultDto<CreateMovieResponseDto>
            {
                Items = result,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }
        public async Task<IEnumerable<TopRentedMovieDto>> GetTopRentedMovies(int count)
        {
            var movies = await _movieRepository.GetAll();

            if (movies == null)
                return new List<TopRentedMovieDto>();

            var topMovies = movies
                .OrderByDescending(m => m.RentalCount)
                .Take(count)
                .ToList();

            var result = topMovies.Select(m => new TopRentedMovieDto
            {
                MovieId = m.Id,
                Title = m.Title,
                RentalCount = m.RentalCount,
                ReleaseYear = m.ReleaseYear,
                Language = m.Language
            });

            return result;
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