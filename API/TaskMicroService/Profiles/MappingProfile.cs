using AutoMapper;
using TaskMicroService.dto;
using TaskMicroService.Entities;

namespace TaskMicroService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping TaskEntity to ReadTaskDto, including the Steps
            CreateMap<TaskEntity, ReadTaskDto>()
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps));

            // Mapping from CreateTaskDto to TaskEntity for creating new tasks
            CreateMap<CreateTaskDto, TaskEntity>();

            // Mapping from UpdateTaskDto to TaskEntity for updating existing tasks
            CreateMap<UpdateTaskDto, TaskEntity>();
            
            // Mapping for TaskStep entities
            CreateMap<TaskStep, ReadTaskStepDto>();    // Maps TaskStep to ReadTaskStepDto
            CreateMap<CreateTaskStepDto, TaskStep>();  // Maps CreateTaskStepDto to TaskStep for creation
            CreateMap<UpdateTaskStepDto, TaskStep>();  // Maps UpdateTaskStepDto to TaskStep for updates
        }
    }
}