using UserMicroService.dto;
using AutoMapper;
using UserMicroService.Entities;

namespace UserMicroService.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateUserDto, ApplicationUser>();
        CreateMap<ApplicationUser, ReadUserDto>();
        CreateMap<UpdateUserDto, ApplicationUser>();
    }
}
