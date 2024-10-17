using UserService.dto;
using AutoMapper;
using DataAccess;

namespace UserService.Service;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public UserService(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public IEnumerable<ReadUserDto> GetAllUsers()
    {
        var user = _dbContext.Users.ToList();
        return _mapper.Map<List<ReadUserDto>>(user);
    }

    public ReadUserDto GetUserById(int id)
    {
        throw new NotImplementedException();
    }

    public int CreateUser(CreateUserDto createUserDto)
    {
        throw new NotImplementedException();
    }

    public void DeleteUser(int id)
    {
        throw new NotImplementedException();
    }

    public void UpdateTask(int id, ReadUserDto readUserDto)
    {
        throw new NotImplementedException();
    }
}