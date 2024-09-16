using API.Dto;
using DataAccess.Models;

namespace API
{
    public static class TaskMapper
    {
        public static TaskToDo MapToTaskToDo(TaskDto taskDto)
            {
                return new TaskToDo 
                {
                    Name = taskDto.Name,
                    IsCompleted = taskDto.IsCompleted
                };
            }

        public static TaskDto MapToTaskDto(TaskToDo taskToDo)
        {
            if (taskToDo == null)
                return null;
            
            return new TaskDto
            {
                Name = taskToDo.Name,
                IsCompleted = taskToDo.IsCompleted
            };
        }
     }
}