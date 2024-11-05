namespace OverblikPlus.Services
{
    public interface ITaskService
    {
        
        Task<List<ReadTaskDto>> GetAllTasks();
        Task<List<ReadTaskDto>> GetTasksForUserAsync(int userId);
        
        Task<List<ReadTaskDto>> GetTasksForTodayAsync();
    }
}