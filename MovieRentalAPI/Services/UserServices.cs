using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Repositories;
using MovieRentalModels;

namespace MovieRentalAPI.Services
{
    public class UserServices : Repository<int, User>, IUserServices
    {
       // private readonly IRepository<int, User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IRepository<int, Rental> _rentalRepository;
        private readonly IRepository<int, RentalItem> _rentalitemRepository;
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, User> _userRepository;

        public UserServices(MovieRentalContext context,
                            IPasswordService passwordService,
                            ITokenService tokenService, IRepository<int,Rental> rentalRepository, IRepository<int, Movie> movieRepository,IRepository<int,User> userRepository, IRepository<int, RentalItem> rentalitemRepository) : base(context)
        {
            //_userRepository = userRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _rentalRepository=rentalRepository;
            _rentalitemRepository=rentalitemRepository;
            _movieRepository=movieRepository;
            _userRepository = userRepository;
        }

        public async Task<CheckUserResponseDto> CheckUser(CheckUserRequestDto request)
        {
            var user =await base.FirstOrDefaultAsync(u => u.Username == request.Username);
            //var user = await _context.FindAsync(u => u.Name = request.Username);
            if (user == null)
                throw new UnAuthorizedException("Invalid username");
            byte[] userPasswordHash = _passwordService.HashPassword(request.Password, user.PasswordHash, hashkey: out var newhash);
            for (int i = 0; i < userPasswordHash.Length; i++)
            {
                if (userPasswordHash[i] != user.Password[i])
                    throw new UnAuthorizedException("Invalid password");
            }
            var tokenpaload = new TokenPayloadDto
            {
                Username = user.Username,
                Role = user.Role
            };
            var token = _tokenService.CreateToken(tokenpaload);
            return new CheckUserResponseDto
            {
                Username=request.Username,
                Token = token
            };
        }

        public async Task<RegisterUserResponseDto> RegisterUser(RegisterUserRequestDto request)
        {
            var existingUser = await base.FirstOrDefaultAsync(u => u.Username == request.Username); 
            if (existingUser != null)
                throw new Exception("Username already exists");

            byte[] passwordHash =
                _passwordService.HashPassword(request.Password, null, out var key);

            User user = new User
            {
                Username = request.Username,
                Password = passwordHash,
                PasswordHash = key,
                Role = "Customer",
                Name=request.Name,
                Email=request.Email,
                Phone=request.Phone
            };

            var addedUser = await base.Add(user);
            if (addedUser == null)
                throw new Exception("User registration failed");

            return new RegisterUserResponseDto
            {
                Username = addedUser.Username,
                Message = "User registered successfully"
            };
        }
        public async Task<IEnumerable<UserRentedMovieResponseDto>> GetAllRentedMovies(int userId)
        {
            var rentals = (await _rentalRepository.GetAll())
                ?.Where(r => r.UserId == userId)
                .ToList();

            if (rentals == null || rentals.Count == 0)
                return new List<UserRentedMovieResponseDto>();

            var rentalItems = await _rentalitemRepository.GetAll();

            var result = new List<UserRentedMovieResponseDto>();

            foreach (var rental in rentals)
            {
                var items = rentalItems
                    ?.Where(i => i.RentalId == rental.Id)
                    .ToList();

                if (items == null) continue;

                foreach (var item in items)
                {
                    var movie = await _movieRepository.Get(item.MovieId);

                    result.Add(new UserRentedMovieResponseDto
                    {
                        RentalId = rental.Id,
                        RentalItemId = item.Id,
                        MovieId = item.MovieId,
                        MovieTitle = movie?.Title ?? "",
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        PricePerDay = item.PricePerDay,
                        IsActive = item.IsActive,
                        RentalStatus = rental.Status
                    });
                }
            }

            return result;
        }
        public async Task<UserResponseDto?> GetUserById(int id)
        {
            var user = await _userRepository.Get(id);

            if (user == null)
                return null;

            return MapToResponse(user);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsers()
        {
            var users = await _userRepository.GetAll();

            if (users == null)
                return new List<UserResponseDto>();

            return users.Select(MapToResponse);
        }

        public async Task<UserResponseDto?> UpdateUser(int id, UpdateUserRequestDto request)
        {
            var existing = await _userRepository.Get(id);

            if (existing == null)
                return null;

            existing.Username = request.Username;
            existing.Role = request.Role;

            var updated = await _userRepository.Update(id, existing);

            return updated == null ? null : MapToResponse(updated);
        }

        public async Task<bool> DeleteUser(int id)
        {
            var deleted = await _userRepository.Delete(id);

            return deleted != null;
        }

        private UserResponseDto MapToResponse(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Email=user.Email,
                Phone=user.Phone
            };
        }
    }
}