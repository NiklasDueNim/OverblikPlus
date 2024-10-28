using UserMicroService.dto;
using AutoMapper;
using UserMicroService.Entities;

namespace UserMicroService.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserEntity, ReadUserDto>();   // Til at læse brugere
        CreateMap<CreateUserDto, UserEntity>(); // Til at oprette brugere
        CreateMap<UpdateUserDto, UserEntity>(); // Til at opdatere brugere
    }
}