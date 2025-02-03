using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.dtos.Task;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class RelativeService : IRelativeService
{
    private readonly TaskDbContext _dbContext;
    private readonly IMapper _mapper;

    public RelativeService(TaskDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<IEnumerable<ReadTaskDto>> GetTasksForDayForSpecificUser(string userId, DateTime date)
    {
        var tasks =  await _dbContext.Tasks.Where(t => t.UserId == userId && t.StartDate.Date == date.Date).ToListAsync();
        var mappedTasks = _mapper.Map<IEnumerable<ReadTaskDto>>(tasks);
        
        return (mappedTasks);
        
  
    }
}