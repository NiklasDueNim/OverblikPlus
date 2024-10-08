using API.Controllers;
using API.Dto;
using DataAccess;
using DataAccess.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
        public IEnumerable<ReadTaskDto> GetAllTasks()
        {
            var task = _dbContext.Tasks.ToList();
            return _mapper.Map<List<ReadTaskDto>>(task);
        }

        public ReadTaskDto GetTaskById(int id)
        {
            var task = _dbContext.Tasks
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);
            return _mapper.Map<ReadTaskDto>(task);
        }

        public int CreateTask(CreateTaskDto createTaskDto)
        {
            var task = _mapper.Map<TaskEntity>(createTaskDto);
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

        public void UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            var taskEntity = _dbContext.Tasks.FirstOrDefault(t => t.Id == id);

            if (taskEntity != null)
            {
                _mapper.Map(updateTaskDto, taskEntity);
                _dbContext.SaveChanges();
            }
        }
    }
}
