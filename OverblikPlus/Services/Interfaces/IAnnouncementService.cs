using OverblikPlus.Models.Dtos.Announcement;

namespace OverblikPlus.Services.Interfaces;

public interface IAnnouncementService
{
    Task<List<AnnouncementDto>> GetAllAsync();
    
    Task CreateAsync(AnnouncementDto dto);
    
    Task UpdateAsync(AnnouncementDto dto);
    
    Task DeleteAsync(Guid id);
}