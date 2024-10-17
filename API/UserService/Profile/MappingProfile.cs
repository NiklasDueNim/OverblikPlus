using DataAccess.Models;
using UserService.dto;

namespace UserService.Profile;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        CreateMap<UserEntity, ReadUserDto>();
    }
}