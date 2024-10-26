using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.dto;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;

namespace TaskMicroService.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskDbContext _dbContext;
        private readonly IMapper _mapper;

        public TaskService(TaskDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReadTaskDto>> GetAllTasks()
        {
            var tasks = await _dbContext.Tasks
                .Include(t => t.Steps) // Include Steps
                .ToListAsync();
            return _mapper.Map<List<ReadTaskDto>>(tasks);
        }

        public async Task<ReadTaskDto> GetTaskById(int id)
        {
            var task = await _dbContext.Tasks
                .Include(t => t.Steps)
                .FirstOrDefaultAsync(t => t.Id == id);

            return _mapper.Map<ReadTaskDto>(task);
        }

        public async Task<int> CreateTask(CreateTaskDto createTaskDto)
        {
            var task = _mapper.Map<TaskEntity>(createTaskDto);
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();

            return task.Id;
        }

        public async Task DeleteTask(int id)
        {
            var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task != null)
            {
                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            var taskEntity = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (taskEntity != null)
            {
                _mapper.Map(updateTaskDto, taskEntity);
                await _dbContext.SaveChangesAsync();
            }
        }

        // TaskStep relaterede metoder
        public async Task<List<ReadTaskStepDto>> GetStepsForTask(int taskId)
        {
            var steps = await _dbContext.TaskSteps
                .Where(s => s.TaskId == taskId)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();

            return _mapper.Map<List<ReadTaskStepDto>>(steps);
        }

        public async Task<int> CreateTaskStep(CreateTaskStepDto createStepDto)
        {
            var taskStep = _mapper.Map<TaskStep>(createStepDto);
            _dbContext.TaskSteps.Add(taskStep);
            await _dbContext.SaveChangesAsync();

            return taskStep.Id;
        }

        public async Task<ReadTaskStepDto?> GetTaskStep(int taskId, int stepNumber)
        {
            var step = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.StepNumber == stepNumber);

            return _mapper.Map<ReadTaskStepDto>(step);
        }

        public async Task UpdateTaskStep(int taskId, int stepNumber, UpdateTaskStepDto updateStepDto)
        {
            var step = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.StepNumber == stepNumber);

            if (step != null)
            {
                _mapper.Map(updateStepDto, step);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteTaskStep(int taskId, int stepNumber)
        {
            var step = await _dbContext.TaskSteps
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.StepNumber == stepNumber);

            if (step != null)
            {
                _dbContext.TaskSteps.Remove(step);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
