using AutoMapper;

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