using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;

namespace TaskMicroService.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly TaskDbContext _dbContext;

    public BudgetRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<BudgetEntity>> GetBudgetsByUserIdAsync(string userId)
    {
        return await _dbContext.Budgets
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Date)
            .ToListAsync();
    }

    public async Task<BudgetEntity?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Budgets.FindAsync(id);
    }

    public async Task<BudgetEntity> AddAsync(BudgetEntity budget)
    {
        await _dbContext.Budgets.AddAsync(budget);
        return budget;
    }

    public Task UpdateAsync(BudgetEntity budget)
    {
        _dbContext.Budgets.Update(budget);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(BudgetEntity budget)
    {
        _dbContext.Budgets.Remove(budget);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

