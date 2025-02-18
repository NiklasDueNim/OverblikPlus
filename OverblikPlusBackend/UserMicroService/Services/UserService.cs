using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OverblikPlus.Shared.Interfaces;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Helpers;
using UserMicroService.Services.Interfaces;
using UserMicroService.Common;

namespace UserMicroService.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UserService(UserManager<ApplicationUser> userManager, IMapper mapper, ILoggerService logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IEnumerable<ReadUserDto>>> GetAllUsersAsync()
        {
            _logger.LogInfo("Fetching all users.");
            var users = await _userManager.Users.ToListAsync();
            if (users == null || !users.Any())
            {
                _logger.LogWarning("No users found.");
                return Result<IEnumerable<ReadUserDto>>.ErrorResult("No users found.");
            }
            return Result<IEnumerable<ReadUserDto>>.SuccessResult(_mapper.Map<List<ReadUserDto>>(users));
        }

        public async Task<Result<ReadUserDto>> GetUserById(string id, string userRole)
        {
            _logger.LogInfo($"Fetching user with ID: {id}");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found.");
                return Result<ReadUserDto>.ErrorResult("User not found.");
            }

            if (userRole == "Admin" || userRole == "Staff")
            {
                user.Medication = EncryptionHelper.Decrypt(user.Medication);
            }

            return Result<ReadUserDto>.SuccessResult(_mapper.Map<ReadUserDto>(user));
        }

        public async Task<Result<string>> CreateUserAsync(CreateUserDto createUserDto)
        {
            _logger.LogInfo("Creating new user.");

            var user = _mapper.Map<ApplicationUser>(createUserDto);
            user.UserName = user.FirstName + user.LastName;

            if (!string.IsNullOrEmpty(createUserDto.Medication))
            {
                user.Medication = EncryptionHelper.Encrypt(createUserDto.Medication);
            }

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user.", new Exception(errors));
                return Result<string>.ErrorResult("Failed to create user: " + errors);
            }

            _logger.LogInfo($"User created successfully with ID: {user.Id}");
            return Result<string>.SuccessResult(user.Id);
        }

        public async Task<Result> DeleteUserAsync(string id)
        {
            _logger.LogInfo($"Deleting user with ID: {id}");
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                _logger.LogInfo($"User with ID: {id} deleted successfully.");
                return Result.SuccessResult();
            }
            else
            {
                _logger.LogWarning($"User with ID: {id} not found.");
                return Result.ErrorResult("User not found");
            }
        }

        public async Task<Result<List<ApplicationUser>>> GetAllUsersForBosted(int bostedId)
        {
            var users = await _userManager.Users
                .Where(u => u.BostedId == bostedId)
                .ToListAsync();

            if (users == null || !users.Any())
            {
                return Result<List<ApplicationUser>>.ErrorResult("No users found for the given BostedId");
            }

            return Result<List<ApplicationUser>>.SuccessResult(users);
        }

        public async Task<Result> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            _logger.LogInfo($"Updating user with ID: {id}");
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                _mapper.Map(updateUserDto, user);
                await _userManager.UpdateAsync(user);
                _logger.LogInfo($"User with ID: {id} updated successfully.");
                return Result.SuccessResult();
            }
            else
            {
                _logger.LogWarning($"User with ID: {id} not found.");
                return Result.ErrorResult("User not found");
            }
        }
    }
}