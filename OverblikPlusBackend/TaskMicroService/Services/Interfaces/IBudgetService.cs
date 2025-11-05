using OverblikPlus.Shared.Common;
using TaskMicroService.Dtos.Budget;

namespace TaskMicroService.Services.Interfaces;

public interface IBudgetService
{
    Task<Result<List<ReadBudgetDto>>> GetAllBudgetsAsync(string userId);
    Task<Result<ReadBudgetDto>> GetBudgetByIdAsync(Guid id);
    Task<Result<ReadBudgetDto>> CreateBudgetAsync(CreateBudgetDto createBudgetDto);
    Task<Result<ReadBudgetDto>> UpdateBudgetAsync(Guid id, UpdateBudgetDto updateBudgetDto);
    Task<Result> DeleteBudgetAsync(Guid id);
}
