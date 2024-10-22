namespace OverblikPlus.Services
{
    public interface ITaskService
    {
        // Metode til at hente alle opgaver
        Task<List<TaskDto>> GetAllTasksAsync();

        // Metode til at hente specifikke opgaver for en bruger
        Task<List<TaskDto>> GetTasksForUserAsync(int userId);

        // Metode til at hente dagens opgaver
        Task<List<TaskDto>> GetTasksForTodayAsync();
    }
}