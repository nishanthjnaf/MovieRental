using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Services
{
    public class WatchlistService : IWatchlistService
    {
        private readonly IRepository<int, Watchlist> _watchlistRepo;
        private readonly IRepository<int, Movie> _movieRepo;
        private readonly IRepository<int, User> _userRepo;

        public WatchlistService(
            IRepository<int, Watchlist> watchlistRepo,
            IRepository<int, Movie> movieRepo,
            IRepository<int, User> userRepo)
        {
            _watchlistRepo = watchlistRepo;
            _movieRepo = movieRepo;
            _userRepo = userRepo;
        }

        public async Task<WatchlistResponseDto> AddToWatchlist(WatchlistRequestDto request)
        {
            if (request.UserId <= 0 || request.MovieId <= 0)
                throw new BadRequestException("Invalid user or movie id");

            var user = await _userRepo.Get(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            var movie = await _movieRepo.Get(request.MovieId);
            if (movie == null)
                throw new NotFoundException("Movie not found");

            var existing = (await _watchlistRepo.GetAll())
                ?.FirstOrDefault(w =>
                    w.UserId == request.UserId &&
                    w.MovieId == request.MovieId);

            if (existing != null)
                throw new ConflictException("Movie already exists in watchlist");

            var watchlist = new Watchlist
            {
                UserId = request.UserId,
                MovieId = request.MovieId
            };

            var added = await _watchlistRepo.Add(watchlist);

            if (added == null)
                throw new Exception("Failed to add to watchlist");

            return new WatchlistResponseDto
            {
                Id = added.Id,
                UserId = added.UserId,
                MovieId = added.MovieId,
                MovieTitle = movie.Title
            };
        }

        public async Task<IEnumerable<WatchlistResponseDto>> GetUserWatchlist(int userId)
        {
            var user = await _userRepo.Get(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            var list = await _watchlistRepo.GetAll();

            var userList = list?
                .Where(w => w.UserId == userId)
                .ToList();

            if (userList == null || !userList.Any())
                throw new NotFoundException("Watchlist is empty");

            var result = new List<WatchlistResponseDto>();

            foreach (var item in userList)
            {
                var movie = await _movieRepo.Get(item.MovieId);

                result.Add(new WatchlistResponseDto
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    MovieId = item.MovieId,
                    MovieTitle = movie?.Title ?? ""
                });
            }

            return result;
        }

        public async Task<bool> RemoveFromWatchlist(int id)
        {
            var existing = await _watchlistRepo.Get(id);

            if (existing == null)
                throw new NotFoundException("Watchlist item not found");

            var deleted = await _watchlistRepo.Delete(id);

            if (deleted == null)
                throw new Exception("Failed to remove watchlist item");

            return true;
        }
    }
}