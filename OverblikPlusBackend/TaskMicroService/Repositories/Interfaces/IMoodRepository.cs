using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface IMoodRepository
{
    Task<MoodEntity?> GetMoodByUserIdAndDateAsync(string userId, DateTime date);
    Task<List<MoodEntity>> GetMoodsForUserAsync(string userId, DateTime fromDate, DateTime toDate);
    Task<List<MoodEntity>> GetMoodsForUsersAsync(List<string> userIds, DateTime fromDate, DateTime toDate);
    Task<MoodEntity> AddAsync(MoodEntity mood);
    Task UpdateAsync(MoodEntity mood);
    Task SaveChangesAsync();
}

