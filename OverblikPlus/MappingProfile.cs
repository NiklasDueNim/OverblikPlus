using AutoMapper;
using OverblikPlus.Dtos.Tasks;
using OverblikPlus.Dtos.TaskSteps;
using OverblikPlus.Dtos.User;
using OverblikPlus.Entities;

namespace OverblikPlus
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ReadTaskDto, UpdateTaskDto>();
            CreateMap<ReadTaskStepDto, UpdateTaskStepDto>();
            CreateMap<UpdateTaskStepDto, ReadTaskStepDto>();
            CreateMap<User, ReadUserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}