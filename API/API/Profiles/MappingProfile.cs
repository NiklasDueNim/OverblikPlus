using API.Dto;
using AutoMapper;
using DataAccess.Models;

namespace API.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TaskToDo, TaskDto>(); //Read
            CreateMap<CreateTaskDto, TaskToDo>(); //Create
        }
    }
}



