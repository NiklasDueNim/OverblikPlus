using AutoMapper;
using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Dtos.Shift;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class ShiftService : IShiftService
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public ShiftService(IShiftRepository shiftRepository, IMapper mapper, ILoggerService logger)
    {
        _shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ReadShiftDto>>> GetShiftsForDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        _logger.LogInfo($"Getting shifts from {fromDate.Date} to {toDate.Date}");

        try
        {
            var shifts = await _shiftRepository.GetShiftsForDateRangeAsync(fromDate, toDate);
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
            var shifts = await _shiftRepository.GetShiftsForUserAsync(userId, fromDate, toDate);
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

            await _shiftRepository.AddAsync(shiftEntity);
            await _shiftRepository.SaveChangesAsync();

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
            var shift = await _shiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                _logger.LogWarning($"Shift with id {shiftId} not found");
                return Result.ErrorResult("Shift not found");
            }

            await _shiftRepository.DeleteAsync(shift);
            await _shiftRepository.SaveChangesAsync();

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
