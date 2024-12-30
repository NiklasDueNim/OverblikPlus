using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using OverblikPlus.Shared.Interfaces;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Helpers;
using UserMicroService.Services;

namespace UserMicroService.Tests.UnitTests;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userManagerMock = MockUserManager(new List<ApplicationUser>());
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _userService = new UserService(_userManagerMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnList_WhenUsersExist()
        {
            // Arrange
            var users = new List<ApplicationUser> 
            { 
                new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe" } 
            }.AsQueryable();

            var userDtos = new List<ReadUserDto> 
            { 
                new ReadUserDto { Id = "1", FirstName = "John", LastName = "Doe" } 
            };
            
            _userManagerMock.Setup(x => x.Users).Returns(users.BuildMock().BuildMockDbSet().Object);
            _mapperMock.Setup(x => x.Map<List<ReadUserDto>>(users)).Returns(userDtos);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("John", result.First().FirstName);
        }


        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            EncryptionHelper.SetEncryptionKey("TestEncryptionKey123456123456789");

            var user = new ApplicationUser
            {
                Id = "1",
                FirstName = "John",
                LastName = "Doe",
                Medication = EncryptionHelper.Encrypt("Med1") 
            };
            var userDto = new ReadUserDto
            {
                Id = "1",
                FirstName = "John",
                LastName = "Doe"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mapperMock.Setup(x => x.Map<ReadUserDto>(user)).Returns(userDto);

            // Act
            var result = await _userService.GetUserById("1", "Admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnUserId_WhenSuccessful()
        {
            // Arrange
            EncryptionHelper.SetEncryptionKey("TestEncryptionKey123456123456789");
            var createUserDto = new CreateUserDto 
            { 
                FirstName = "John", 
                LastName = "Doe", 
                Password = "Password123", 
                Medication = "Med1" 
            };

            var user = new ApplicationUser 
            { 
                Id = "1", 
                FirstName = "John", 
                LastName = "Doe", 
                Medication = EncryptionHelper.Encrypt("Med1")
            };

            _mapperMock.Setup(x => x.Map<ApplicationUser>(createUserDto)).Returns(user);
            _userManagerMock.Setup(x => x.CreateAsync(user, createUserDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result);
        }


        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1" };
            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _userService.DeleteUserAsync("1");

            // Assert
            _userManagerMock.Verify(x => x.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            EncryptionHelper.SetEncryptionKey("TestEncryptionKey123456123456789");

            var user = new ApplicationUser
            {
                Id = "1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Medication = EncryptionHelper.Encrypt("Med1", true),
                Goals = "Goal1"
            };

            var updateUserDto = new UpdateUserDto
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Medication = "Med2",
                Goals = "Goal2"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);

            _mapperMock.Setup(x => x.Map(It.IsAny<UpdateUserDto>(), It.IsAny<ApplicationUser>()))
                .Callback<UpdateUserDto, ApplicationUser>((dto, u) =>
                {
                    u.FirstName = dto.FirstName;
                    u.LastName = dto.LastName;
                    u.Email = dto.Email;
                    u.Medication = EncryptionHelper.Encrypt(dto.Medication, true);
                    u.Goals = dto.Goals;
                });

            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _userService.UpdateUserAsync("1", updateUserDto);

            // Assert
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);

            Assert.Equal("Jane", user.FirstName);
            Assert.Equal("Smith", user.LastName);
            Assert.Equal("jane.smith@example.com", user.Email);
            Assert.Equal("Med2", EncryptionHelper.Decrypt(user.Medication, true));
            Assert.Equal("Goal2", user.Goals);
        }
        

        private static Mock<UserManager<ApplicationUser>> MockUserManager(List<ApplicationUser> users)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(x => x.Users).Returns(users.AsQueryable());
            return userManager;
        }
}
