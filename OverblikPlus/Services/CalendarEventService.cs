using System.Net.Http.Json;
using OverblikPlus.Models.Dtos.Calendar;
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
        try
        {
            var response = await _httpClient.GetAsync($"api/CalendarEvent/user/{userId}");
            response.EnsureSuccessStatusCode();

            var events = await response.Content.ReadFromJsonAsync<IEnumerable<ReadCalendarEventDto>>();
            if (events == null)
            {
                throw new Exception("Failed to deserialize events.");
            }

            return events;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching events for user {userId}: {ex.Message}");
            throw;
        }
    }

    public async Task<ReadCalendarEventDto?> GetEventByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/CalendarEvent/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ReadCalendarEventDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching event with ID {id}: {ex.Message}");
            throw;
        }
    }

    public async Task CreateEventAsync(CreateCalendarEventDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/CalendarEvent", dto);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating event: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateEventAsync(int id, CreateCalendarEventDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/CalendarEvent/{id}", dto);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating event with ID {id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteEventAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/CalendarEvent/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting event with ID {id}: {ex.Message}");
            throw;
        }
    }
}