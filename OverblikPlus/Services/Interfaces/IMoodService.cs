using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Mood;

namespace OverblikPlus.Services.Interfaces;

public interface IMoodService
{
    Task<Result<List<MoodDto>>> GetMoodsForUserAsync(Guid userId, DateTime fromDate, DateTime toDate);
    Task<Result> CreateMood(MoodDto mood);
    Task<Result<List<MoodWithUserDto>>> GetMoodsForBostedAsync(int bostedId, DateTime fromDate, DateTime toDate);
}