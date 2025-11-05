using OverblikPlus.Shared.Common;
using TaskMicroService.Dtos.Shift;

namespace TaskMicroService.Services.Interfaces;

public interface IShiftService
{
    Task<Result<List<ReadShiftDto>>> GetShiftsForDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<Result<List<ReadShiftDto>>> GetShiftsForUserAsync(string userId, DateTime fromDate, DateTime toDate);
    Task<Result<ReadShiftDto>> CreateShiftAsync(CreateShiftDto createShiftDto);
    Task<Result> DeleteShiftAsync(Guid shiftId);
}
