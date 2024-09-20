using API.Dto;
using DataAccess.Models;

namespace API
{
    public static class TaskMapper
    {
        public static TaskEntity MapToTaskToDo(TaskDto taskDto)
            {
                return new TaskEntity 
                {
                    Name = taskDto.Name,
                    IsCompleted = taskDto.IsCompleted
                };
            }

        public static TaskDto MapToTaskDto(TaskEntity taskEntity)
        {
            if (taskEntity == null)
                return null;
            
            return new TaskDto
            {
                Name = taskEntity.Name,
                IsCompleted = taskEntity.IsCompleted
            };
        }
     }
}