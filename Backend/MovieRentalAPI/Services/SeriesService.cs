using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalModels;

namespace MovieRentalAPI.Services
{
    public class SeriesService : ISeriesService
    {
        private readonly MovieRentalContext _context;

        public SeriesService(MovieRentalContext context)
        {
            _context = context;
        }

        public async Task<SeriesResponseDto> AddSeries(SeriesRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new BadRequestException("Series title is required");

            if (await _context.Series.AnyAsync(s => s.Title.ToLower() == request.Title.ToLower()))
                throw new ConflictException("Series already exists");

            var series = new Series
            {
                Title = request.Title,
                Description = request.Description,
                Language = request.Language,
                Director = request.Director ?? string.Empty,
                Cast = request.Cast?.Trim() ?? string.Empty,
                ContentRating = request.ContentRating ?? string.Empty,
                ContentAdvisory = request.ContentAdvisory?.Trim() ?? string.Empty,
                PosterPath = request.PosterPath,
                TrailerUrl = request.TrailerUrl,
                RentalPrice = request.RentalPrice,
                IsAvailable = request.IsAvailable,
                RentalCount = 0,
                Genres = new List<Genre>()
            };

            if (request.GenreIds != null)
            {
                foreach (var gid in request.GenreIds)
                {
                    var g = new Genre { Id = gid };
                    _context.Attach(g);
                    series.Genres.Add(g);
                }
            }

            _context.Series.Add(series);
            await _context.SaveChangesAsync();

            foreach (var sReq in request.Seasons)
            {
                var season = new Season
                {
                    SeriesId = series.Id,
                    SeasonNumber = sReq.SeasonNumber,
                    Title = sReq.Title,
                    ReleaseYear = sReq.ReleaseYear,
                    Episodes = sReq.Episodes.Select(e => new Episode
                    {
                        EpisodeNumber = e.EpisodeNumber,
                        Title = e.Title,
                        Description = e.Description,
                        DurationMinutes = e.DurationMinutes,
                        AirDate = e.AirDate ?? DateTime.UtcNow
                    }).ToList()
                };
                _context.Seasons.Add(season);
            }

            await _context.SaveChangesAsync();

            return await GetSeriesById(series.Id);
        }

        public async Task<SeriesResponseDto> GetSeriesById(int id)
        {
            var series = await _context.Series
                .Include(s => s.Genres)
                .Include(s => s.Seasons)
                    .ThenInclude(s => s.Episodes)
                .Include(s => s.Seasons)
                    .ThenInclude(s => s.Reviews)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (series == null)
                throw new NotFoundException("Series not found");

            return MapToDto(series);
        }

        public async Task<IEnumerable<SeriesResponseDto>> GetAllSeries()
        {
            var list = await _context.Series
                .Include(s => s.Genres)
                .Include(s => s.Seasons)
                    .ThenInclude(s => s.Episodes)
                .Include(s => s.Seasons)
                    .ThenInclude(s => s.Reviews)
                .ToListAsync();

            return list.Select(MapToDto);
        }

        public async Task<SeriesResponseDto> UpdateSeries(int id, SeriesRequestDto request)
        {
            var series = await _context.Series
                .Include(s => s.Genres)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (series == null)
                throw new NotFoundException("Series not found");

            series.Title = request.Title;
            series.Description = request.Description;
            series.Language = request.Language;
            series.Director = request.Director ?? string.Empty;
            series.Cast = request.Cast?.Trim() ?? string.Empty;
            series.ContentRating = request.ContentRating ?? string.Empty;
            series.ContentAdvisory = request.ContentAdvisory?.Trim() ?? string.Empty;
            series.PosterPath = request.PosterPath;
            series.TrailerUrl = request.TrailerUrl;
            series.RentalPrice = request.RentalPrice;
            series.IsAvailable = request.IsAvailable;

            series.Genres.Clear();
            if (request.GenreIds != null)
            {
                foreach (var gid in request.GenreIds)
                {
                    var g = new Genre { Id = gid };
                    _context.Attach(g);
                    series.Genres.Add(g);
                }
            }

            await _context.SaveChangesAsync();
            return await GetSeriesById(id);
        }

        public async Task<bool> DeleteSeries(int id)
        {
            var series = await _context.Series.FindAsync(id);
            if (series == null)
                throw new NotFoundException("Series not found");

            _context.Series.Remove(series);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SeriesResponseDto>> GetNewSeries(int count)
        {
            var list = await _context.Series
                .Include(s => s.Genres)
                .Include(s => s.Seasons).ThenInclude(s => s.Episodes)
                .Include(s => s.Seasons).ThenInclude(s => s.Reviews)
                .OrderByDescending(s => s.Id)
                .Take(count)
                .ToListAsync();

            return list.Select(MapToDto);
        }

        public async Task<IEnumerable<SeriesResponseDto>> GetTopRatedSeries(int count)
        {
            var list = await _context.Series
                .Include(s => s.Genres)
                .Include(s => s.Seasons).ThenInclude(s => s.Episodes)
                .Include(s => s.Seasons).ThenInclude(s => s.Reviews)
                .ToListAsync();

            return list
                .Select(s => new { Series = s, Avg = AverageSeriesRating(s) })
                .OrderByDescending(x => x.Avg)
                .Take(count)
                .Select(x => MapToDto(x.Series));
        }

        public async Task<IEnumerable<SeriesResponseDto>> GetTopRentedSeries(int count)
        {
            var list = await _context.Series
                .Include(s => s.Genres)
                .Include(s => s.Seasons).ThenInclude(s => s.Episodes)
                .Include(s => s.Seasons).ThenInclude(s => s.Reviews)
                .OrderByDescending(s => s.RentalCount)
                .Take(count)
                .ToListAsync();

            return list.Select(MapToDto);
        }

        public async Task<IEnumerable<SeriesResponseDto>> GetSuggestedSeries(int userId)
        {
            var pref = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (pref == null || !pref.IsSet)
                return await GetTopRatedSeries(10);

            var genres = pref.PreferredGenres.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim().ToLower()).ToHashSet();
            var langs = pref.PreferredLanguages.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim().ToLower()).ToHashSet();

            var list = await _context.Series
                .Include(s => s.Genres)
                .Include(s => s.Seasons).ThenInclude(s => s.Episodes)
                .Include(s => s.Seasons).ThenInclude(s => s.Reviews)
                .ToListAsync();

            return list
                .Where(s =>
                    (genres.Count == 0 || s.Genres.Any(g => genres.Contains(g.Name.ToLower()))) ||
                    (langs.Count == 0 || langs.Contains(s.Language.ToLower())))
                .OrderByDescending(s => AverageSeriesRating(s))
                .Take(10)
                .Select(MapToDto);
        }

        public async Task<SeasonResponseDto> AddSeason(AddSeasonRequestDto request)
        {
            var series = await _context.Series.FindAsync(request.SeriesId);
            if (series == null) throw new NotFoundException("Series not found");

            var season = new Season
            {
                SeriesId = request.SeriesId,
                SeasonNumber = request.SeasonNumber,
                Title = request.Title,
                ReleaseYear = request.ReleaseYear,
                Episodes = request.Episodes.Select(e => new Episode
                {
                    EpisodeNumber = e.EpisodeNumber,
                    Title = e.Title,
                    Description = e.Description,
                    DurationMinutes = e.DurationMinutes,
                    AirDate = e.AirDate ?? DateTime.UtcNow
                }).ToList()
            };

            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();

            return new SeasonResponseDto
            {
                Id = season.Id,
                SeriesId = season.SeriesId,
                SeasonNumber = season.SeasonNumber,
                Title = season.Title,
                ReleaseYear = season.ReleaseYear,
                AverageRating = 0,
                IsNewSeason = true,
                Episodes = season.Episodes.Select(e => new EpisodeResponseDto
                {
                    Id = e.Id, SeasonId = e.SeasonId, EpisodeNumber = e.EpisodeNumber,
                    Title = e.Title, Description = e.Description,
                    DurationMinutes = e.DurationMinutes, AirDate = e.AirDate
                }).ToList()
            };
        }

        public async Task<EpisodeResponseDto> AddEpisode(AddEpisodeRequestDto request)
        {
            var season = await _context.Seasons.FindAsync(request.SeasonId);
            if (season == null) throw new NotFoundException("Season not found");

            var episode = new Episode
            {
                SeasonId = request.SeasonId,
                EpisodeNumber = request.EpisodeNumber,
                Title = request.Title,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                AirDate = request.AirDate ?? DateTime.UtcNow
            };

            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();

            return new EpisodeResponseDto
            {
                Id = episode.Id, SeasonId = episode.SeasonId,
                EpisodeNumber = episode.EpisodeNumber, Title = episode.Title,
                Description = episode.Description, DurationMinutes = episode.DurationMinutes,
                AirDate = episode.AirDate
            };
        }

        private static double AverageSeriesRating(Series s)
        {
            if (s.Seasons == null || !s.Seasons.Any()) return 0;
            var allRatings = s.Seasons
                .Where(season => season.Reviews != null && season.Reviews.Any())
                .SelectMany(season => season.Reviews!)
                .Select(r => r.Rating)
                .ToList();
            return allRatings.Any() ? allRatings.Average() : 0;
        }

        private static SeriesResponseDto MapToDto(Series s)
        {
            return new SeriesResponseDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Language = s.Language,
                Director = s.Director,
                Cast = string.IsNullOrWhiteSpace(s.Cast)
                    ? new List<string>()
                    : s.Cast.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList(),
                ContentRating = s.ContentRating,
                ContentAdvisory = string.IsNullOrWhiteSpace(s.ContentAdvisory)
                    ? new List<string>()
                    : s.ContentAdvisory.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList(),
                PosterPath = s.PosterPath,
                TrailerUrl = s.TrailerUrl,
                RentalPrice = s.RentalPrice,
                IsAvailable = s.IsAvailable,
                RentalCount = s.RentalCount,
                Genres = s.Genres?.Select(g => g.Name).ToList() ?? new List<string>(),
                Seasons = s.Seasons?.OrderBy(sn => sn.SeasonNumber).Select(sn => new SeasonResponseDto
                {
                    Id = sn.Id,
                    SeriesId = sn.SeriesId,
                    SeasonNumber = sn.SeasonNumber,
                    Title = sn.Title,
                    ReleaseYear = sn.ReleaseYear,
                    AverageRating = sn.Reviews != null && sn.Reviews.Any()
                        ? sn.Reviews.Average(r => r.Rating)
                        : 0,
                    IsNewSeason = sn.Episodes != null && sn.Episodes.Any()
                        && sn.Episodes.Max(e => e.AirDate) >= DateTime.UtcNow.AddDays(-30),
                    Episodes = sn.Episodes?.OrderBy(e => e.EpisodeNumber).Select(e => new EpisodeResponseDto
                    {
                        Id = e.Id,
                        SeasonId = e.SeasonId,
                        EpisodeNumber = e.EpisodeNumber,
                        Title = e.Title,
                        Description = e.Description,
                        DurationMinutes = e.DurationMinutes,
                        AirDate = e.AirDate
                    }).ToList() ?? new List<EpisodeResponseDto>()
                }).ToList() ?? new List<SeasonResponseDto>()
            };
        }
    }
}
