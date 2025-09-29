using OverblikPlus.Models.Dtos.Mood;

namespace OverblikPlus.Services.Interfaces;

public interface IMoodService
{
    Task<List<MoodDto>> GetMoodsForUserAsync(Guid userId, DateTime fromDate, DateTime toDate);
    Task CreateMood(MoodDto mood);
}