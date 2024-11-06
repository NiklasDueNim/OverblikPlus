using AutoMapper;
using OverblikPlus.Dtos;
using OverblikPlus.Dtos.User;
using OverblikPlus.Entities;

namespace OverblikPlus
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, ReadUserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}