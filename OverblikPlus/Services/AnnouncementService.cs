using System.Net;
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
    public Task<List<AnnouncementDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task CreateAsync(AnnouncementDto dto)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(AnnouncementDto dto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}