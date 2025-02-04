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
        var tasks = await _dbContext.Tasks
            .Where(t => t.UserId == userId && t.StartDate.Date == date.Date)
            .ToListAsync();

        if (tasks == null || !tasks.Any())
        {
            return Enumerable.Empty<ReadTaskDto>();
        }

        var mappedTasks = _mapper.Map<IEnumerable<ReadTaskDto>>(tasks);

        if (mappedTasks == null || !mappedTasks.Any())
        {
            throw new InvalidOperationException("Mapping from TaskEntity to ReadTaskDto failed or resulted in an empty list.");
        }
        
        foreach (var task in mappedTasks)
        {
            Console.WriteLine($"Mapped Task: {task.Name}, {task.Image}, {task.Steps}");
        }

        return mappedTasks;
    }
}