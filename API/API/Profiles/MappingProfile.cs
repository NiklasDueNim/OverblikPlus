using API.Dto;
using AutoMapper;
using DataAccess.Models;

namespace API.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TaskEntity, ReadTaskDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username)); //Read
            CreateMap<CreateTaskDto, TaskEntity>(); //Create
            CreateMap<UpdateTaskDto, TaskEntity>(); // Update
            CreateMap<UserEntity, UserDto>();
        }
    }
}



