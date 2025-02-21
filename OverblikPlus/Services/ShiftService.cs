using OverblikPlus.Models.Dtos.Shift;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class ShiftService : IShiftService
{
    private readonly HttpClient _httpClient;

    public ShiftService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<List<ShiftDto>> GetShifts(DateTime from, DateTime to)
    {
        throw new NotImplementedException();
    }

    public Task CreateShiftAsync(ShiftDto shift)
    {
        throw new NotImplementedException();
    }
}