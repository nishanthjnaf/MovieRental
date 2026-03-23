using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Helpers;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Repositories;
using MovieRentalModels;

namespace MovieRentalAPI.Services
{
    public class UserServices : Repository<int, User>, IUserServices
    {
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IRepository<int, Rental> _rentalRepository;
        private readonly IRepository<int, RentalItem> _rentalitemRepository;
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, User> _userRepository;

        public UserServices(
            MovieRentalContext context,
            IPasswordService passwordService,
            ITokenService tokenService,
            IRepository<int, Rental> rentalRepository,
            IRepository<int, Movie> movieRepository,
            IRepository<int, User> userRepository,
            IRepository<int, RentalItem> rentalitemRepository)
            : base(context)
        {
            _tokenService = tokenService;
            _passwordService = passwordService;
            _rentalRepository = rentalRepository;
            _movieRepository = movieRepository;
            _userRepository = userRepository;
            _rentalitemRepository = rentalitemRepository;
        }

        public async Task<CheckUserResponseDto> CheckUser(CheckUserRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                throw new BadRequestException("Username and password are required");

            var user = await base.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                throw new UnAuthorizedException("Invalid username");

            byte[] userPasswordHash =
                _passwordService.HashPassword(request.Password, user.PasswordHash, out var newhash);

            for (int i = 0; i < userPasswordHash.Length; i++)
            {
                if (userPasswordHash[i] != user.Password[i])
                    throw new UnAuthorizedException("Invalid password");
            }

            var tokenPayload = new TokenPayloadDto
            {
                Username = user.Username,
                Role = user.Role
            };

            var token = _tokenService.CreateToken(tokenPayload);

            return new CheckUserResponseDto
            {
                Username = user.Username,
                Token = token
            };
        }

        public async Task<RegisterUserResponseDto> RegisterUser(RegisterUserRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Email))
                throw new BadRequestException("Invalid registration details");

            var existingUser =
                await base.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
                throw new ConflictException("Username already exists");

            byte[] passwordHash =
                _passwordService.HashPassword(request.Password, null, out var key);

            var user = new User
            {
                Username = request.Username,
                Password = passwordHash,
                PasswordHash = key!,
                Role = "Customer",
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone
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
            var user = await _userRepository.Get(userId);

            if (user == null)
                throw new NotFoundException("User not found");

            var rentals = (await _rentalRepository.GetAll())
                ?.Where(r => r.UserId == userId)
                .ToList();

            if (rentals == null || rentals.Count == 0)
                throw new NotFoundException("No movie is rented so far");

            var rentalItems = await _rentalitemRepository.GetAll();

            var result = new List<UserRentedMovieResponseDto>();

            foreach (var rental in rentals)
            {
                var items = rentalItems?
                    .Where(i => i.RentalId == rental.Id)
                    .ToList();

                if (items == null || items.Count == 0)
                    continue;

                foreach (var item in items)
                {
                    if (item.IsActive && item.EndDate <= IstDateTime.Now)
                    {
                        item.IsActive = false;
                        await _rentalitemRepository.Update(item.Id, item);
                    }

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

        public async Task<UserResponseDto> GetUserById(int id)
        {
            var user = await _userRepository.Get(id);

            if (user == null)
                throw new NotFoundException("User not found");

            return MapToResponse(user);
        }

        public async Task<UserResponseDto> GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new BadRequestException("Username is required");

            var user = await base.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                throw new NotFoundException("User not found");

            return MapToResponse(user);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsers()
        {
            var users = await _userRepository.GetAll();

            if (users == null || !users.Any())
                throw new NotFoundException("No users found");

            return users.Select(MapToResponse);
        }

        public async Task<UserResponseDto> UpdateUser(int id, UpdateUserRequestDto request)
        {
            var existing = await _userRepository.Get(id);

            if (existing == null)
                throw new NotFoundException("User not found");

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new BadRequestException("Username cannot be empty");

            existing.Username = request.Username;
            existing.Role = request.Role;
            existing.Name = request.Name;
            existing.Email = request.Email;
            existing.Phone = request.Phone;

            var updated = await _userRepository.Update(id, existing);

            if (updated == null)
                throw new Exception("User update failed");

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteUser(int id)
        {
            var existing = await _userRepository.Get(id);

            if (existing == null)
                throw new NotFoundException("User not found");

            var deleted = await _userRepository.Delete(id);

            if (deleted == null)
                throw new Exception("User deletion failed");

            return true;
        }

        public async Task<bool> ResetPassword(int id, ResetPasswordRequestDto request)
        {
            var existing = await _userRepository.Get(id);
            if (existing == null)
                throw new NotFoundException("User not found");

            if (string.IsNullOrWhiteSpace(request.OldPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.ConfirmPassword))
                throw new BadRequestException("All password fields are required");

            if (request.NewPassword != request.ConfirmPassword)
                throw new BadRequestException("New password and confirm password do not match");

            byte[] oldHash = _passwordService.HashPassword(
                request.OldPassword,
                existing.PasswordHash,
                out var oldKey);

            for (int i = 0; i < oldHash.Length; i++)
            {
                if (oldHash[i] != existing.Password[i])
                    throw new BadRequestException("Old password is incorrect");
            }

            byte[] newHash = _passwordService.HashPassword(
                request.NewPassword,
                null,
                out var newKey);

            existing.Password = newHash;
            existing.PasswordHash = newKey!;

            var updated = await _userRepository.Update(id, existing);
            if (updated == null)
                throw new Exception("Failed to reset password");

            return true;
        }

        private UserResponseDto MapToResponse(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                Role = user.Role,
                Email = user.Email,
                Phone = user.Phone
            };
        }
    }
}