using AutoMapper;
using TaskMicroService.dto;
using TaskMicroService.Entities;

namespace TaskMicroService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping TaskEntity to ReadTaskDto, including the Username from the related User entity
            CreateMap<TaskEntity, ReadTaskDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty)); // Read

            // Mapping from CreateTaskDto to TaskEntity for creating new tasks
            CreateMap<CreateTaskDto, TaskEntity>();

            // Mapping from UpdateTaskDto to TaskEntity for updating existing tasks
            CreateMap<UpdateTaskDto, TaskEntity>();
        }
    }
}