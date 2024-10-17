using System.Collections.Generic;
using API.Dto;


namespace API.Services;

public interface IUserService
{
    ReadUserDto GetUserById(int id);
    
    IEnumerable<ReadUserDto> GetAllUsers();

    int Createuser(CreateUserDto createUserDto);
    
    void DeleteUser(int id);
    

}