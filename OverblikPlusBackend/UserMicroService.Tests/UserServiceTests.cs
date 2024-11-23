using Microsoft.AspNetCore.Identity;
using Moq;
using UserMicroService.Services;
using UserMicroService.Entities;
using AutoMapper;
using UserMicroService.dto;

public class UserServiceTests
{
    private Mock<UserManager<ApplicationUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null
        );
    }

    [Fact]
    public async Task CreateUser_ShouldReturnUserIdAsync()
    {
        var userManagerMock = GetMockUserManager();
        var mapperMock = new Mock<IMapper>();
        var userService = new UserService(userManagerMock.Object, mapperMock.Object);

        mapperMock.Setup(m => m.Map<ApplicationUser>(It.IsAny<CreateUserDto>()))
            .Returns((CreateUserDto dto) => new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Medication = dto.Medication,
                Role = dto.Role,
                Email = dto.Email
            });
        
        var createUserDto = new CreateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Medication = "None",
            Role = "User",
            Password = "Password123!",
            Email = "john.doe@example.com"
        };
        
        userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        var result = await userService.CreateUserAsync(createUserDto);
        
        
        Assert.False(string.IsNullOrEmpty(result));
    }

}
