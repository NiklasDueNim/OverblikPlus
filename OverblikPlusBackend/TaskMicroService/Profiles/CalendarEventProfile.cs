using AutoMapper;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.Entities;

namespace TaskMicroService.Profiles;

public class CalendarEventProfile : Profile
{
    public CalendarEventProfile()
    {
        CreateMap<CreateCalendarEventDto, CalendarEvent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<CalendarEvent, ReadCalendarEventDto>();
    }
}