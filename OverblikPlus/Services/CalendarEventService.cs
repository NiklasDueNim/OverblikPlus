using System.Net.Http.Json;
using OverblikPlus.Dtos.Calendar;

namespace OverblikPlus.Services;

public class CalendarEventService
{
    private readonly HttpClient _httpClient;

    public CalendarEventService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Hent alle events for en bestemt bruger
    public async Task<IEnumerable<ReadCalendarEventDto>> GetAllEventsAsync(string userId)
    {
        var response = await _httpClient.GetAsync($"api/CalendarEvent/user/{userId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<ReadCalendarEventDto>>();
    }

    // Hent et event baseret på ID
    public async Task<ReadCalendarEventDto?> GetEventByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/CalendarEvent/{id}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ReadCalendarEventDto>();
    }

    // Opret et nyt event
    public async Task CreateEventAsync(CreateCalendarEventDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/CalendarEvent", dto);
        response.EnsureSuccessStatusCode();
    }

    // Opdater et eksisterende event
    public async Task UpdateEventAsync(int id, CreateCalendarEventDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/CalendarEvent/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    // Slet et event
    public async Task DeleteEventAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/CalendarEvent/{id}");
        response.EnsureSuccessStatusCode();
    }
}
