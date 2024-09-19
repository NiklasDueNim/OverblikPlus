using API.Controllers;
using API.Dto;
using DataAccess;
using DataAccess.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public TaskService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public IEnumerable<TaskDto> GetAllTasks()
        {
            var task = _dbContext.Tasks.ToList();
            return _mapper.Map<List<TaskDto>>(task);
        }

        public TaskDto GetTaskById(int id)
        {
            var task = _dbContext.Tasks.FirstOrDefault(t => t.Id == id);
            return _mapper.Map<TaskDto>(task);
        }

        public int CreateTask(CreateTaskDto createTaskDto)
        {
            var task = _mapper.Map<TaskToDo>(createTaskDto);
            _dbContext.Tasks.Add(task);
            _dbContext.SaveChanges();

            return task.Id;
        }

        public void DeleteTask(int id)
        {
            var task = _dbContext.Tasks.FirstOrDefault(t => t.Id == id);
            
            _dbContext.Remove(task);
            _dbContext.SaveChanges();
        }

        public void UpdateTask(TaskDto task)
        {
            throw new NotImplementedException();
        }
    }
}
