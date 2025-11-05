using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Shift;

namespace OverblikPlus.Services.Interfaces;

public interface IShiftService
{
    Task<Result<List<ShiftDto>>> GetShifts(DateTime from, DateTime to);
    Task<Result> CreateShiftAsync(ShiftDto shift);
    Task<Result> DeleteShiftAsync(Guid shiftId);
}