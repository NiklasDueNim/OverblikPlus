using Microsoft.AspNetCore.Identity;
using Moq;
using UserMicroService.Services;
using UserMicroService.Entities;
using Xunit;
using AutoMapper;
using UserMicroService.dto;

public class UserServiceTests
{
    private UserManager<ApplicationUser> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new UserManager<ApplicationUser>(
            store.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
    }

    [Fact]
    public async Task CreateUser_ShouldReturnUserIdAsync()
    {
        // Arrange
        var userManager = GetMockUserManager();
        var mapper = new Mock<IMapper>();
        var userService = new UserService(userManager, mapper.Object);

        // Sæt op mapperen til at mappe korrekt
        mapper.Setup(m => m.Map<ApplicationUser>(It.IsAny<CreateUserDto>()))
            .Returns((CreateUserDto dto) => new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Medication = dto.Medication,
                Goals = dto.Goals,
                UserName = dto.Username,
                Email = dto.Email
            });

        var createUserDto = new CreateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.Now,
            Medication = "None",
            Goals = "Complete project",
            Username = "johndoe",
            Password = "Password123!",
            Email = "john.doe@example.com"
        };

        // Act
        var result = await userService.CreateUserAsync(createUserDto);

        // Assert
        Assert.True(result > 0); // Ensure that a user ID is returned
    }
}