using OverblikPlus.Models.Dtos.Facilities;

namespace OverblikPlus.Services.Interfaces;

public interface IFacilityService
{
    Task<List<ReadFacilityDto>> GetAllAsync();
    Task<bool> CreateAsync(CreateFacilityDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateFacilityDto dto);
    Task<bool> DeleteAsync(Guid id);
}
