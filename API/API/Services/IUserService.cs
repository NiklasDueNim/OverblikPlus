using API.Dto;
using DataAccess.Models;

namespace API.Services;

public interface IUserService
{
    ReadUserDto GetUserById(int id);
    
    IEnumerable<ReadUserDto> GetAllUsers();

    int Createuser(CreateUserDto createUserDto);
    
    void DeleteUser(int id);
    

}