using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;

namespace TaskMicroService.Repositories;

public class FacilityRepository : IFacilityRepository
{
    private readonly TaskDbContext _dbContext;

    public FacilityRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<FacilityEntity>> GetAllAsync() =>
        await _dbContext.Facilities.OrderBy(f => f.Name).ToListAsync();

    public async Task<FacilityEntity?> GetByIdAsync(Guid id) =>
        await _dbContext.Facilities.FirstOrDefaultAsync(f => f.Id == id);

    public async Task<FacilityEntity> AddAsync(FacilityEntity facility)
    {
        await _dbContext.Facilities.AddAsync(facility);
        return facility;
    }

    public async Task UpdateAsync(FacilityEntity facility)
    {
        _dbContext.Facilities.Update(facility);
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task DeleteAsync(FacilityEntity facility)
    {
        _dbContext.Facilities.Remove(facility);
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
