using System.Net;
using OverblikPlus.Models.Dtos.Mood;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class MoodService : IMoodService
{
    private readonly HttpClient _httpClient;

    public MoodService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public Task<List<MoodDto>> GetMoodsForUserAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
        throw new NotImplementedException();
    }

    public Task CreateMood(MoodDto mood)
    {
        throw new NotImplementedException();
    }
}