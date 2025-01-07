using AutoMapper;
using TaskMicroService.dtos.Task;
using TaskMicroService.dtos.TaskStep;
using TaskMicroService.Entities;

namespace TaskMicroService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Task mappings
            CreateMap<TaskEntity, ReadTaskDto>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps));


            CreateMap<CreateTaskDto, TaskEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Steps, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.NextOccurrence, opt => opt.Ignore());
                
            
            
            
            CreateMap<UpdateTaskDto, TaskEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Steps, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());
            
          
            // TaskStep mappings
            CreateMap<TaskStep, ReadTaskStepDto>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.ImageUrl));

            CreateMap<CreateTaskStepDto, TaskStep>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NextStepId, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

    
            CreateMap<UpdateTaskStepDto, TaskStep>()
                .ForMember(dest => dest.StepNumber, opt => opt.Ignore())
                .ForMember(dest => dest.NextStepId, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) 
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}