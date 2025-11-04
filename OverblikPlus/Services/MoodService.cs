using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using OverblikPlus.AuthHelpers;
using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Mood;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class MoodService : IMoodService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authStateProvider;

    public MoodService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authStateProvider = (CustomAuthStateProvider)authenticationStateProvider;
    }

    public async Task<Result<List<MoodDto>>> GetMoodsForUserAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var fromDateStr = fromDate.ToString("yyyy-MM-dd");
            var toDateStr = toDate.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetFromJsonAsync<Result<List<MoodDto>>>(
                $"/api/Mood/user/{userId}?fromDate={fromDateStr}&toDate={toDateStr}");

            return response ?? Result<List<MoodDto>>.ErrorResult("No data received");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching moods: {ex.Message}");
            return Result<List<MoodDto>>.ErrorResult($"Error fetching moods: {ex.Message}");
        }
    }

    public async Task<Result> CreateMood(MoodDto mood)
    {
        try
        {
            // Convert MoodDto to backend CreateMoodDto format
            var createMoodDto = new
            {
                UserId = mood.UserId,
                Date = mood.Date,
                Rating = (int)mood.Rating,
                Note = mood.Note ?? string.Empty
            };

            var response = await _httpClient.PostAsJsonAsync("/api/Mood", createMoodDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<object>>();
                if (result?.Success == true)
                {
                    return Result.SuccessResult();
                }
                return Result.ErrorResult(result?.Error ?? "Failed to create mood");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to create mood. Status: {response.StatusCode}, Error: {errorContent}");
            return Result.ErrorResult($"Failed to create mood: {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating mood: {ex.Message}");
            return Result.ErrorResult($"Error creating mood: {ex.Message}");
        }
    }

    public async Task<Result<List<MoodWithUserDto>>> GetMoodsForBostedAsync(int bostedId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var fromDateStr = fromDate.ToString("yyyy-MM-dd");
            var toDateStr = toDate.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetFromJsonAsync<Result<List<MoodWithUserDto>>>(
                $"/api/Mood/bosted/{bostedId}?fromDate={fromDateStr}&toDate={toDateStr}");

            return response ?? Result<List<MoodWithUserDto>>.ErrorResult("No data received");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching moods for bosted: {ex.Message}");
            return Result<List<MoodWithUserDto>>.ErrorResult($"Error fetching moods for bosted: {ex.Message}");
        }
    }
}