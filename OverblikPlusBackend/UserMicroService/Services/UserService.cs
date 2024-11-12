using UserMicroService.dto;
using AutoMapper;
using UserMicroService.DataAccess;
using UserMicroService.Entities;
using UserMicroService.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserMicroService.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserService(UserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReadUserDto>> GetAllUsersAsync()
        {
            var users = await _dbContext.Users.ToListAsync();
            return _mapper.Map<List<ReadUserDto>>(users);
        }
        public async Task<ReadUserDto> GetUserById(int id, string userRole)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return null;
            }
            
            if ((userRole == "Admin" || userRole == "Staff") && !string.IsNullOrEmpty(user.CPRNumber))
            {
                user.CPRNumber = EncryptionHelper.Decrypt(user.CPRNumber);
            }

            if ((userRole == "Admin" || userRole == "Staff") && !string.IsNullOrEmpty(user.MedicationDetails))
            {
                user.MedicationDetails = EncryptionHelper.Decrypt(user.MedicationDetails);
            }

            return _mapper.Map<ReadUserDto>(user);
        }

        public async Task<int> CreateUserAsync(CreateUserDto createUserDto)
        {
            if (createUserDto == null)
            {
                throw new ArgumentNullException(nameof(createUserDto), "CreateUserDto cannot be null");
            }

            var user = _mapper.Map<UserEntity>(createUserDto);
            
            Console.WriteLine($"Before encryption: FirstName = {user.FirstName}, CPRNumber = {user.CPRNumber}, MedicationDetails = {user.MedicationDetails}");

            if (!string.IsNullOrEmpty(user.CPRNumber))
            {
                user.CPRNumber = EncryptionHelper.Encrypt(user.CPRNumber);
            }

            if (!string.IsNullOrEmpty(user.MedicationDetails))
            {
                user.MedicationDetails = EncryptionHelper.Encrypt(user.MedicationDetails);
            }

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return user.Id;
        }

        
        public async Task DeleteUserAsync(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("User not found");
            }
        }

        public async Task UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userEntity != null)
            {
                _mapper.Map(updateUserDto, userEntity);

                if (!string.IsNullOrEmpty(userEntity.CPRNumber))
                {
                    userEntity.CPRNumber = EncryptionHelper.Encrypt(userEntity.CPRNumber);
                }

                if (!string.IsNullOrEmpty(userEntity.MedicationDetails))
                {
                    userEntity.MedicationDetails = EncryptionHelper.Encrypt(userEntity.MedicationDetails);
                }

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("User not found");
            }
        }
    }
}
