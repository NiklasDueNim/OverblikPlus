using AutoMapper;

namespace OverblikPlus
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping mellem dine modeller og DTO'er
            CreateMap<User, ReadUserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}