using OverblikPlus.Shared.Common;
using TaskMicroService.dtos.Facility;

namespace TaskMicroService.Services.Interfaces;

public interface IFacilityService
{
    Task<Result<IEnumerable<ReadFacilityDto>>> GetAllAsync();
    Task<Result<ReadFacilityDto>> GetByIdAsync(Guid id);
    Task<Result<Guid>> CreateAsync(CreateFacilityDto dto);
    Task<Result> UpdateAsync(Guid id, UpdateFacilityDto dto);
    Task<Result> DeleteAsync(Guid id);
}
