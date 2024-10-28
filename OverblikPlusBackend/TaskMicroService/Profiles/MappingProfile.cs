using AutoMapper;
using TaskMicroService.dto;
using TaskMicroService.Entities;

namespace TaskMicroService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Task mapping
            CreateMap<TaskEntity, ReadTaskDto>()
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps)); // Maps Steps collection

            CreateMap<CreateTaskDto, TaskEntity>();
            CreateMap<UpdateTaskDto, TaskEntity>();

            // TaskStep mapping
            CreateMap<TaskStep, ReadTaskStepDto>(); // Maps TaskStep to ReadTaskStepDto
            CreateMap<CreateTaskStepDto, TaskStep>();

            // Tilføj hvis du har brug for en opdatering til TaskStep
            CreateMap<TaskStep, TaskStepDto>();
        }
    }
}