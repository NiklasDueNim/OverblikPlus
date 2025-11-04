using AutoMapper;
using TaskMicroService.dtos.Task;
using TaskMicroService.dtos.TaskStep;
using TaskMicroService.Dtos.Mood;
using TaskMicroService.Dtos.Budget;
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
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps))
                .ForMember(dest => dest.SelectedWeekDays, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.SelectedWeekDays) ? new Dictionary<string, bool>() : 
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(src.SelectedWeekDays, (System.Text.Json.JsonSerializerOptions)null) ?? new Dictionary<string, bool>()));


            CreateMap<CreateTaskDto, TaskEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Steps, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.NextOccurrence, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedWeekDays, opt => opt.MapFrom(src => 
                    System.Text.Json.JsonSerializer.Serialize(src.SelectedWeekDays ?? new Dictionary<string, bool>(), (System.Text.Json.JsonSerializerOptions)null)));
                
            
            
            
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
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            // Mood mappings
            CreateMap<CreateMoodDto, MoodEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<MoodEntity, ReadMoodDto>();

            // Budget mappings
            CreateMap<CreateBudgetDto, BudgetEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<BudgetEntity, ReadBudgetDto>();

            CreateMap<UpdateBudgetDto, BudgetEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

        }
    }
}