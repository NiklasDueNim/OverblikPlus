using UserService.dto;

namespace UserService.Service;

public interface IUserService
{
    IEnumerable<ReadUserDto> GetAllUsers();
    
    ReadUserDto GetUserById(int id);

    int CreateUser(CreateUserDto createUserDto);
    
    void DeleteUser(int id);

    void UpdateTask(int id, ReadUserDto readUserDto); 


}