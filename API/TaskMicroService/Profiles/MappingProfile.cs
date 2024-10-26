using AutoMapper;
using TaskMicroService.dto;
using TaskMicroService.Entities;

namespace TaskMicroService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping TaskEntity to ReadTaskDto, including mapping Steps to TaskStepDto
            CreateMap<TaskEntity, ReadTaskDto>()
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps)); // Maps Steps collection

            // Mapping from CreateTaskDto to TaskEntity for creating new tasks
            CreateMap<CreateTaskDto, TaskEntity>();

            // Mapping from UpdateTaskDto to TaskEntity for updating existing tasks
            CreateMap<UpdateTaskDto, TaskEntity>();
            
            // Mapping between TaskStep and TaskStepDto
            CreateMap<TaskStep, TaskStepDto>();  // Ensure this line is included
            
            CreateMap<TaskStepDto, TaskStep>();
        }
    }
}