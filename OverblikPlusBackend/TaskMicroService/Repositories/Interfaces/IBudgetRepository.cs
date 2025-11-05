using TaskMicroService.Entities;

namespace TaskMicroService.Repositories.Interfaces;

public interface IBudgetRepository
{
    Task<List<BudgetEntity>> GetBudgetsByUserIdAsync(string userId);
    Task<BudgetEntity?> GetByIdAsync(Guid id);
    Task<BudgetEntity> AddAsync(BudgetEntity budget);
    Task UpdateAsync(BudgetEntity budget);
    Task DeleteAsync(BudgetEntity budget);
    Task SaveChangesAsync();
}

