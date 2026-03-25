using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalModels;
using Microsoft.EntityFrameworkCore;

namespace MovieRentalAPI.Services
{
    public class MovieServices : IMovieServices
    {
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, Genre> _genreRepository;
        private readonly IRepository<int, Review> _reviewRepository;
        private readonly MovieRentalContext _context;

        public MovieServices(
            IRepository<int, Movie> movieRepository,
            IRepository<int, Genre> genreRepository,
            IRepository<int, Review> reviewRepository,
            MovieRentalContext context)
        {
            _movieRepository = movieRepository;
            _genreRepository = genreRepository;
            _reviewRepository = reviewRepository;
            _context = context;
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
                Director = request.Director ?? string.Empty,
                Cast = request.Cast?.Trim() ?? string.Empty,
                ContentRating = request.ContentRating ?? string.Empty,
                ContentAdvisory = request.ContentAdvisory?.Trim() ?? string.Empty,
                Genres = new List<Genre>(),
                PosterPath = request.PosterPath,
                TrailerUrl = request.TrailerUrl,
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
            var movie = await _movieRepository.GetIncluding(id, m => m.Genres);

            if (movie == null)
                throw new NotFoundException("Movie not found");

            var avgByMovieId = await GetAverageRatingsByMovieId();
            if (avgByMovieId.TryGetValue(movie.Id, out var avg))
                movie.Rating = avg;

            return MapToResponseDto(movie);
        }

        public async Task<IEnumerable<CreateMovieResponseDto>> GetAllMovies()
        {
            var movies = await _movieRepository.GetAllIncluding(m => m.Genres);

            if (movies == null || !movies.Any())
                throw new NotFoundException("No movies found");

            var avgByMovieId = await GetAverageRatingsByMovieId();

            foreach (var m in movies)
            {
                if (avgByMovieId.TryGetValue(m.Id, out var avg))
                    m.Rating = avg;
            }

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
            existingMovie.PosterPath = request.PosterPath;
            existingMovie.TrailerUrl = request.TrailerUrl;
            existingMovie.Director = request.Director ?? string.Empty;
            existingMovie.Cast = request.Cast?.Trim() ?? string.Empty;
            existingMovie.ContentRating = request.ContentRating ?? string.Empty;
            existingMovie.ContentAdvisory = request.ContentAdvisory?.Trim() ?? string.Empty;

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

            var movies = await _movieRepository.GetAllIncluding(m => m.Genres);

            if (movies == null || !movies.Any())
                throw new NotFoundException("No movies found");

            var avgByMovieId = await GetAverageRatingsByMovieId();
            foreach (var m in movies)
            {
                if (avgByMovieId.TryGetValue(m.Id, out var avg))
                    m.Rating = avg;
            }

            var filtered = movies
                .Where(m =>
                    string.IsNullOrEmpty(request.SearchTerm) ||
                    m.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (m.Director ?? string.Empty).Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (m.Cast ?? string.Empty).Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase))
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
                    Language = m.Language,
                    PosterPath=m.PosterPath,
                    TrailerUrl=m.TrailerUrl
                })
                .ToList();
        }

        public async Task<IEnumerable<CreateMovieResponseDto>> GetTopUserRatedMovies(int count)
        {
            if (count <= 0)
                throw new BadRequestException("Count must be greater than zero");

            var movies = await _movieRepository.GetAll();

            if (movies == null || !movies.Any())
                throw new NotFoundException("No movies found");

            // Ensure correct ordering even if existing Movie.Rating values are stale.
            var allReviews = await _reviewRepository.GetAll();
            var avgByMovieId = allReviews?
                .GroupBy(r => r.MovieId)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

            avgByMovieId ??= new Dictionary<int, double>();

            return movies
                .Select(m =>
                {
                    avgByMovieId.TryGetValue(m.Id, out var avg);
                    m.Rating = avg;
                    return m;
                })
                .Where(m => m.Rating > 0)
                .OrderByDescending(m => m.Rating)
                .Take(count)
                .Select(MapToResponseDto)
                .ToList();
        }

        public async Task<IEnumerable<CreateMovieResponseDto>> GetSuggestedMovies(int userId)
        {
            var pref = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

            var movies = await _movieRepository.GetAllIncluding(m => m.Genres);
            if (movies == null || !movies.Any())
                return Enumerable.Empty<CreateMovieResponseDto>();

            var avgByMovieId = await GetAverageRatingsByMovieId();
            foreach (var m in movies)
            {
                if (avgByMovieId.TryGetValue(m.Id, out var avg))
                    m.Rating = avg;
            }

            IEnumerable<Movie> filtered = movies;

            if (pref != null && pref.IsSet)
            {
                var genres = string.IsNullOrWhiteSpace(pref.PreferredGenres)
                    ? new List<string>()
                    : pref.PreferredGenres.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().ToLower()).ToList();

                var languages = string.IsNullOrWhiteSpace(pref.PreferredLanguages)
                    ? new List<string>()
                    : pref.PreferredLanguages.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().ToLower()).ToList();

                filtered = movies.Where(m =>
                    (genres.Count == 0 || m.Genres.Any(g => genres.Contains(g.Name.ToLower()))) &&
                    (languages.Count == 0 || languages.Contains(m.Language.ToLower()))
                );
            }

            return filtered
                .OrderByDescending(m => m.RentalCount)
                .ThenByDescending(m => m.Rating)
                .Take(10)
                .Select(MapToResponseDto)
                .ToList();
        }

        private CreateMovieResponseDto MapToResponseDto(Movie movie)
        {
            static List<string> SplitCsv(string? value)
            {
                if (string.IsNullOrWhiteSpace(value)) return new List<string>();
                return value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
            }

            return new CreateMovieResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseYear = movie.ReleaseYear,
                DurationMinutes = movie.DurationMinutes,
                Language = movie.Language,
                Rating = movie.Rating,
                Director = movie.Director ?? string.Empty,
                Cast = SplitCsv(movie.Cast),
                ContentRating = movie.ContentRating ?? string.Empty,
                Genres = movie.Genres?.Select(g => g.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>(),
                ContentAdvisory = SplitCsv(movie.ContentAdvisory),
                PosterPath = movie.PosterPath,
                TrailerUrl = movie.TrailerUrl,
                RentalCount = movie.RentalCount
            };
        }

        private async Task<Dictionary<int, double>> GetAverageRatingsByMovieId()
        {
            var allReviews = await _reviewRepository.GetAll();
            if (allReviews == null)
                return new Dictionary<int, double>();

            return allReviews
                .GroupBy(r => r.MovieId)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));
        }
    }
}