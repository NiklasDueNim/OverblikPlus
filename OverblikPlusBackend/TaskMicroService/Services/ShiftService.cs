using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Common;
using TaskMicroService.DataAccess;
using TaskMicroService.Dtos.Shift;
using TaskMicroService.Entities;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class ShiftService : IShiftService
{
    private readonly TaskDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public ShiftService(TaskDbContext dbContext, IMapper mapper, ILoggerService logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ReadShiftDto>>> GetShiftsForDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        _logger.LogInfo($"Getting shifts from {fromDate.Date} to {toDate.Date}");

        try
        {
            var shifts = await _dbContext.Shifts
                .Where(s => s.StartTime.Date >= fromDate.Date && s.StartTime.Date <= toDate.Date)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var shiftDtos = _mapper.Map<List<ReadShiftDto>>(shifts);
            return Result<List<ReadShiftDto>>.SuccessResult(shiftDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting shifts: {ex.Message}", ex);
            return Result<List<ReadShiftDto>>.ErrorResult($"Error getting shifts: {ex.Message}");
        }
    }

    public async Task<Result<List<ReadShiftDto>>> GetShiftsForUserAsync(string userId, DateTime fromDate, DateTime toDate)
    {
        _logger.LogInfo($"Getting shifts for user {userId} from {fromDate.Date} to {toDate.Date}");

        try
        {
            var shifts = await _dbContext.Shifts
                .Where(s => s.UserId == userId && s.StartTime.Date >= fromDate.Date && s.StartTime.Date <= toDate.Date)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var shiftDtos = _mapper.Map<List<ReadShiftDto>>(shifts);
            return Result<List<ReadShiftDto>>.SuccessResult(shiftDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting shifts for user: {ex.Message}", ex);
            return Result<List<ReadShiftDto>>.ErrorResult($"Error getting shifts for user: {ex.Message}");
        }
    }

    public async Task<Result<ReadShiftDto>> CreateShiftAsync(CreateShiftDto createShiftDto)
    {
        _logger.LogInfo($"Creating shift for user {createShiftDto.UserId}");

        try
        {
            if (createShiftDto.EndTime <= createShiftDto.StartTime)
            {
                return Result<ReadShiftDto>.ErrorResult("Sluttid skal være efter starttid");
            }

            var shiftEntity = _mapper.Map<ShiftEntity>(createShiftDto);
            shiftEntity.Id = Guid.NewGuid();

            _dbContext.Shifts.Add(shiftEntity);
            await _dbContext.SaveChangesAsync();

            _logger.LogInfo($"Shift created successfully with ID {shiftEntity.Id}");
            var shiftDto = _mapper.Map<ReadShiftDto>(shiftEntity);
            return Result<ReadShiftDto>.SuccessResult(shiftDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating shift: {ex.Message}", ex);
            return Result<ReadShiftDto>.ErrorResult($"Error creating shift: {ex.Message}");
        }
    }

    public async Task<Result> DeleteShiftAsync(Guid shiftId)
    {
        _logger.LogInfo($"Deleting shift with id {shiftId}");

        try
        {
            var shift = await _dbContext.Shifts.FindAsync(shiftId);
            if (shift == null)
            {
                _logger.LogWarning($"Shift with id {shiftId} not found");
                return Result.ErrorResult("Shift not found");
            }

            _dbContext.Shifts.Remove(shift);
            await _dbContext.SaveChangesAsync();

            _logger.LogInfo($"Shift deleted successfully with ID {shiftId}");
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting shift with id {shiftId}: {ex.Message}", ex);
            return Result.ErrorResult($"Error deleting shift: {ex.Message}");
        }
    }
}
