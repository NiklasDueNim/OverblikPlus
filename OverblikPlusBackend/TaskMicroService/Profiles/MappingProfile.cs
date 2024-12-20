using AutoMapper;
using TaskMicroService.dto;
using TaskMicroService.Entities;

namespace TaskMicroService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Task mappings
            CreateMap<TaskEntity, ReadTaskDto>()
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps));

            CreateMap<CreateTaskDto, TaskEntity>();
            
            // CreateMap<UpdateTaskDto, TaskEntity>();
            
            CreateMap<UpdateTaskDto, TaskEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.NextOccurrence, opt => opt.Ignore())
                .ForMember(dest => dest.Steps, opt => opt.Ignore());
            
          
            // TaskStep mappings
            CreateMap<TaskStep, ReadTaskStepDto>();

            CreateMap<CreateTaskStepDto, TaskStep>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.ImageBase64) ? Convert.FromBase64String(src.ImageBase64) : null));

    
            CreateMap<UpdateTaskStepDto, TaskStep>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) 
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}