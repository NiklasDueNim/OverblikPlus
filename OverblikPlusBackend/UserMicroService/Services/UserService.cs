using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Helpers;

namespace UserMicroService.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UserService(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReadUserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return _mapper.Map<List<ReadUserDto>>(users);
        }

        public async Task<ReadUserDto> GetUserById(int id, string userRole)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;
            
            if (userRole == "Admin" || userRole == "Staff")
            {
                user.Medication = EncryptionHelper.Decrypt(user.Medication);
            }

            return _mapper.Map<ReadUserDto>(user);
        }

        public async Task<string> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = _mapper.Map<ApplicationUser>(createUserDto);
            
            user.Medication = EncryptionHelper.Encrypt(createUserDto.Medication);

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return user.Id;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            else
            {
                throw new KeyNotFoundException("User not found");
            }
        }

        public async Task UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user != null)
            {
                _mapper.Map(updateUserDto, user);
                await _userManager.UpdateAsync(user);
            }
            else
            {
                throw new KeyNotFoundException("User not found");
            }
        }
    }
}
