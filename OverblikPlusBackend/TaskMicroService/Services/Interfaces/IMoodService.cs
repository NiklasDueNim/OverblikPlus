using TaskMicroService.Common;
using TaskMicroService.Dtos.Mood;

namespace TaskMicroService.Services.Interfaces;

public interface IMoodService
{
    Task<Result<ReadMoodDto>> CreateMood(CreateMoodDto createMoodDto);
    Task<Result<List<ReadMoodDto>>> GetMoodsForUserAsync(string userId, DateTime fromDate, DateTime toDate);
    Task<Result<List<ReadMoodWithUserDto>>> GetMoodsForBostedAsync(int bostedId, DateTime fromDate, DateTime toDate);
}
