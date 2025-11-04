using OverblikPlus.Models.Dtos.Budget;

namespace OverblikPlus.Services.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetDto>> GetAllBudgetsAsync();
    Task<BudgetDto?> GetBudgetByIdAsync(Guid id);
    Task<BudgetDto?> CreateBudgetAsync(BudgetDto budget);
    Task<BudgetDto?> UpdateBudgetAsync(Guid id, BudgetDto budget);
    Task<bool> DeleteBudgetAsync(Guid id);
    Task<string?> UploadVoucherAsync(Stream fileStream, string fileName);
}
