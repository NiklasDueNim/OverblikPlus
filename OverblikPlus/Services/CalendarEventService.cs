using System.Net.Http.Json;
using OverblikPlus.Dtos.Calendar;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class CalendarEventService : ICalendarEventService
{
    private readonly HttpClient _httpClient;

    public CalendarEventService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ReadCalendarEventDto>> GetAllEventsAsync(string userId)
    {
        var response = await _httpClient.GetAsync($"api/CalendarEvent/user/{userId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<ReadCalendarEventDto>>();
    }

    public async Task<ReadCalendarEventDto?> GetEventByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/CalendarEvent/{id}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ReadCalendarEventDto>();
    }
    
    public async Task CreateEventAsync(CreateCalendarEventDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/CalendarEvent", dto);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task UpdateEventAsync(int id, CreateCalendarEventDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/CalendarEvent/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEventAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/CalendarEvent/{id}");
        response.EnsureSuccessStatusCode();
    }
}
