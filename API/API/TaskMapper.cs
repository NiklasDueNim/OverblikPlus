using API.Dto;
using DataAccess.Models;

namespace API
{
    public static class TaskMapper
    {
        public static TaskEntity MapToTaskToDo(ReadTaskDto readTaskDto)
            {
                return new TaskEntity 
                {
                    Name = readTaskDto.Name,
                    IsCompleted = readTaskDto.IsCompleted
                };
            }

        public static ReadTaskDto MapToTaskDto(TaskEntity taskEntity)
        {
            if (taskEntity == null)
                return null;
            
            return new ReadTaskDto
            {
                Name = taskEntity.Name,
                IsCompleted = taskEntity.IsCompleted
            };
        }
     }
}