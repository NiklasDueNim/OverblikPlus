using DataAccess.Models;
using AutoMapper;
using TaskService.dto;


namespace TaskService.Profile;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TaskEntity, ReadTaskDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username)); //Read
        CreateMap<CreateTaskDto, TaskEntity>(); //Create
        CreateMap<UpdateTaskDto, TaskEntity>(); // Update
            
    }
    
}