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
            var user = await _userRepo.Get(request.UserId);
            var movie = await _movieRepo.Get(request.MovieId);

            if (user == null || movie == null)
                throw new Exception("Invalid User or Movie");

            var existing = (await _watchlistRepo.GetAll())
                ?.FirstOrDefault(w => w.UserId == request.UserId && w.MovieId == request.MovieId);

            if (existing != null)
                throw new Exception("Movie already in watchlist");

            var watchlist = new Watchlist
            {
                UserId = request.UserId,
                MovieId = request.MovieId
            };

            var added = await _watchlistRepo.Add(watchlist);

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
            var list = await _watchlistRepo.GetAll();

            var userList = list
                .Where(w => w.UserId == userId)
                .ToList();

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
            var deleted = await _watchlistRepo.Delete(id);
            return deleted != null;
        }
    }
}
