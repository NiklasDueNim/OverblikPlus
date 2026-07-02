using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface IFacilityRepository
{
    Task<List<FacilityEntity>> GetAllAsync();
    Task<FacilityEntity?> GetByIdAsync(Guid id);
    Task<FacilityEntity> AddAsync(FacilityEntity facility);
    Task UpdateAsync(FacilityEntity facility);
    Task DeleteAsync(FacilityEntity facility);
    Task SaveChangesAsync();
}
