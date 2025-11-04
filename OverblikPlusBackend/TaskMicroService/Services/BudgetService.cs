using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Common;
using TaskMicroService.DataAccess;
using TaskMicroService.Dtos.Budget;
using TaskMicroService.Entities;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class BudgetService : IBudgetService
{
    private readonly TaskDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public BudgetService(TaskDbContext dbContext, IMapper mapper, ILoggerService logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ReadBudgetDto>>> GetAllBudgetsAsync(string userId)
    {
        _logger.LogInfo($"Getting all budgets for user {userId}");
        
        try
        {
            var budgets = await _dbContext.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            var budgetDtos = _mapper.Map<List<ReadBudgetDto>>(budgets);
            return Result<List<ReadBudgetDto>>.SuccessResult(budgetDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting budgets for user {userId}: {ex.Message}", ex);
            return Result<List<ReadBudgetDto>>.ErrorResult($"Error getting budgets: {ex.Message}");
        }
    }

    public async Task<Result<ReadBudgetDto>> GetBudgetByIdAsync(Guid id)
    {
        _logger.LogInfo($"Getting budget with id {id}");
        
        try
        {
            var budget = await _dbContext.Budgets.FindAsync(id);
            if (budget == null)
            {
                _logger.LogWarning($"Budget with id {id} not found");
                return Result<ReadBudgetDto>.ErrorResult("Budget not found");
            }

            var budgetDto = _mapper.Map<ReadBudgetDto>(budget);
            return Result<ReadBudgetDto>.SuccessResult(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting budget with id {id}: {ex.Message}", ex);
            return Result<ReadBudgetDto>.ErrorResult($"Error getting budget: {ex.Message}");
        }
    }

    public async Task<Result<ReadBudgetDto>> CreateBudgetAsync(CreateBudgetDto createBudgetDto)
    {
        _logger.LogInfo($"Creating budget for user {createBudgetDto.UserId}");
        
        try
        {
            var budgetEntity = _mapper.Map<BudgetEntity>(createBudgetDto);
            budgetEntity.Id = Guid.NewGuid();

            _dbContext.Budgets.Add(budgetEntity);
            await _dbContext.SaveChangesAsync();

            _logger.LogInfo($"Budget created successfully with ID {budgetEntity.Id}");
            var budgetDto = _mapper.Map<ReadBudgetDto>(budgetEntity);
            return Result<ReadBudgetDto>.SuccessResult(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating budget: {ex.Message}", ex);
            return Result<ReadBudgetDto>.ErrorResult($"Error creating budget: {ex.Message}");
        }
    }

    public async Task<Result<ReadBudgetDto>> UpdateBudgetAsync(Guid id, UpdateBudgetDto updateBudgetDto)
    {
        _logger.LogInfo($"Updating budget with id {id}");
        
        try
        {
            var budget = await _dbContext.Budgets.FindAsync(id);
            if (budget == null)
            {
                _logger.LogWarning($"Budget with id {id} not found");
                return Result<ReadBudgetDto>.ErrorResult("Budget not found");
            }

            _mapper.Map(updateBudgetDto, budget);
            await _dbContext.SaveChangesAsync();

            _logger.LogInfo($"Budget updated successfully with ID {id}");
            var budgetDto = _mapper.Map<ReadBudgetDto>(budget);
            return Result<ReadBudgetDto>.SuccessResult(budgetDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating budget with id {id}: {ex.Message}", ex);
            return Result<ReadBudgetDto>.ErrorResult($"Error updating budget: {ex.Message}");
        }
    }

    public async Task<Result> DeleteBudgetAsync(Guid id)
    {
        _logger.LogInfo($"Deleting budget with id {id}");
        
        try
        {
            var budget = await _dbContext.Budgets.FindAsync(id);
            if (budget == null)
            {
                _logger.LogWarning($"Budget with id {id} not found");
                return Result.ErrorResult("Budget not found");
            }

            _dbContext.Budgets.Remove(budget);
            await _dbContext.SaveChangesAsync();

            _logger.LogInfo($"Budget deleted successfully with ID {id}");
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting budget with id {id}: {ex.Message}", ex);
            return Result.ErrorResult($"Error deleting budget: {ex.Message}");
        }
    }
}
