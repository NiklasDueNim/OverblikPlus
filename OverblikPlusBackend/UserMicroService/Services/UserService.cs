using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OverblikPlus.Shared.Interfaces;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Helpers;
using UserMicroService.Services.Interfaces;

namespace UserMicroService.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UserService(UserManager<ApplicationUser> userManager, IMapper mapper, ILoggerService logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ReadUserDto>> GetAllUsersAsync()
        {
            _logger.LogInfo("Fetching all users.");
            var users = await _userManager.Users.ToListAsync();
            return _mapper.Map<List<ReadUserDto>>(users);
        }

        public async Task<ReadUserDto> GetUserById(string id, string userRole)
        {
            _logger.LogInfo($"Fetching user with ID: {id}");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID: {id} not found.");
                return null;
            }

            if (userRole == "Admin" || userRole == "Staff")
            {
                user.Medication = EncryptionHelper.Decrypt(user.Medication);
            }

            return _mapper.Map<ReadUserDto>(user);
        }

        public async Task<string> CreateUserAsync(CreateUserDto createUserDto)
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
                throw new Exception("Failed to create user: " + errors);
            }

            _logger.LogInfo($"User created successfully with ID: {user.Id}");
            return user.Id;
        }

        public async Task DeleteUserAsync(string id)
        {
            _logger.LogInfo($"Deleting user with ID: {id}");
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                _logger.LogInfo($"User with ID: {id} deleted successfully.");
            }
            else
            {
                _logger.LogWarning($"User with ID: {id} not found.");
                throw new KeyNotFoundException("User not found");
            }
        }

        public async Task UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            _logger.LogInfo($"Updating user with ID: {id}");
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                _mapper.Map(updateUserDto, user);
                await _userManager.UpdateAsync(user);
                _logger.LogInfo($"User with ID: {id} updated successfully.");
            }
            else
            {
                _logger.LogWarning($"User with ID: {id} not found.");
                throw new KeyNotFoundException("User not found");
            }
        }
    }
}
