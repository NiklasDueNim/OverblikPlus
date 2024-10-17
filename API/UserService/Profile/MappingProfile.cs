using DataAccess.Models;
using UserService.dto;
using AutoMapper;

namespace UserService.Profile;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserEntity, ReadUserDto>();
    }
}