using System.Net;
using System.Text;
using System.Text.Json;
using OverblikPlus.Models.Dtos.Announcement;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly HttpClient _httpClient;

    public AnnouncementService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AnnouncementDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Announcement");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResult<List<AnnouncementDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result?.Data ?? new List<AnnouncementDto>();
            }
            return new List<AnnouncementDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting announcements: {ex.Message}");
            return new List<AnnouncementDto>();
        }
    }

    public async Task CreateAsync(AnnouncementDto dto)
    {
        try
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/Announcement", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error creating announcement: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating announcement: {ex.Message}");
        }
    }

    public async Task UpdateAsync(AnnouncementDto dto)
    {
        try
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"api/Announcement/{dto.Id}", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error updating announcement: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating announcement: {ex.Message}");
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Announcement/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error deleting announcement: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting announcement: {ex.Message}");
        }
    }
}