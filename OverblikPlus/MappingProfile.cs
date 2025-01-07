using AutoMapper;
using OverblikPlus.Models;
using OverblikPlus.Models.Dtos.Tasks;
using OverblikPlus.Models.Dtos.TaskSteps;
using OverblikPlus.Models.Dtos.User;
using OverblikPlus.Models.FormModels;

namespace OverblikPlus
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ReadTaskDto, UpdateTaskDto>();
            CreateMap<ReadTaskStepDto, UpdateTaskStepDto>();
            CreateMap<UpdateTaskStepDto, ReadTaskStepDto>();

            CreateMap<ReadUserDto, User>(); 
            
            CreateMap<User, ReadUserDto>();
            
            CreateMap<TaskFormModel, CreateTaskDto>();
            CreateMap<TaskFormModel, UpdateTaskDto>();
            CreateMap<ReadTaskDto, TaskFormModel>();

            CreateMap<TaskStepFormModel, CreateTaskStepDto>();
            CreateMap<TaskStepFormModel, UpdateTaskStepDto>();
            CreateMap<ReadTaskStepDto, TaskStepFormModel>();

        }
    }
}