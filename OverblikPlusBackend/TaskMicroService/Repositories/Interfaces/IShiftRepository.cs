using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface IShiftRepository
{
    Task<List<ShiftEntity>> GetShiftsForDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<ShiftEntity>> GetShiftsForUserAsync(string userId, DateTime fromDate, DateTime toDate);
    Task<ShiftEntity?> GetByIdAsync(Guid id);
    Task<ShiftEntity> AddAsync(ShiftEntity shift);
    Task DeleteAsync(ShiftEntity shift);
    Task SaveChangesAsync();
}

