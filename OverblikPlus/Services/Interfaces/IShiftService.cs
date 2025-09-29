using OverblikPlus.Models.Dtos.Shift;

namespace OverblikPlus.Services.Interfaces;

public interface IShiftService
{
    Task<List<ShiftDto>> GetShifts(DateTime from, DateTime to);
    Task CreateShiftAsync(ShiftDto shift);
}