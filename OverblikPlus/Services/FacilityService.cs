using System.Net.Http.Json;
using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Facilities;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class FacilityService : IFacilityService
{
    private readonly HttpClient _httpClient;

    public FacilityService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<List<ReadFacilityDto>> GetAllAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<List<ReadFacilityDto>>>("/api/Facility");
            return result?.Data ?? new List<ReadFacilityDto>();
        }
        catch
        {
            return new List<ReadFacilityDto>();
        }
    }

    public async Task<bool> CreateAsync(CreateFacilityDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Facility", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateFacilityDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/Facility/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/Facility/{id}");
        return response.IsSuccessStatusCode;
    }
}
