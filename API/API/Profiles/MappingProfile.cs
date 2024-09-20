using API.Dto;
using AutoMapper;
using DataAccess.Models;

namespace API.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TaskEntity, TaskDto>(); //Read
            CreateMap<CreateTaskDto, TaskEntity>(); //Create
            CreateMap<UpdateTaskDto, TaskEntity>(); // Update
        }
    }
}



