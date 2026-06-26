using System.Collections;
using System.Net.Http.Json;
using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Calendar;
using OverblikPlus.Models.Dtos.Tasks;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class RelativeService : IRelativeService
{
    private readonly HttpClient _httpClient;

    public RelativeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ReadTaskDto>> GetTasksForDayForSpecificUser(string userId, DateTime date)
    {
        try
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"/api/Relative/{userId}/tasks-for-day?date={formattedDate}");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Error fetching tasks: {response.StatusCode}");
                return new List<ReadTaskDto>();
            }

            // Backend returnerer nu direkte IEnumerable<ReadTaskDto> (unwrapped)
            var tasks = await response.Content.ReadFromJsonAsync<IEnumerable<ReadTaskDto>>();
            return tasks ?? new List<ReadTaskDto>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching tasks: {ex.Message}");
            return new List<ReadTaskDto>();
        }
    }

    public async Task<IEnumerable<ReadCalendarEventDto>> GetEventsForDayForSpecificUser(string userId, DateTime date)
    {
        try
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"/api/Relative/{userId}/events-for-day?date={formattedDate}");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Error fetching events: {response.StatusCode}");
                return new List<ReadCalendarEventDto>();
            }

            // Backend returnerer nu direkte IEnumerable<ReadCalendarEventDto> (unwrapped)
            var events = await response.Content.ReadFromJsonAsync<IEnumerable<ReadCalendarEventDto>>();
            return events ?? new List<ReadCalendarEventDto>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching events: {ex.Message}");
            return new List<ReadCalendarEventDto>();
        }
    }
}