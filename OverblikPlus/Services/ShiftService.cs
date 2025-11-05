using System.Net.Http.Json;
using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.Shift;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

public class ShiftService : IShiftService
{
    private readonly HttpClient _httpClient;

    public ShiftService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<Result<List<ShiftDto>>> GetShifts(DateTime from, DateTime to)
    {
        try
        {
            var fromDateStr = from.ToString("yyyy-MM-dd");
            var toDateStr = to.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetFromJsonAsync<Result<List<ShiftDto>>>(
                $"api/Shift?fromDate={fromDateStr}&toDate={toDateStr}");

            return response ?? Result<List<ShiftDto>>.ErrorResult("No data received");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching shifts: {ex.Message}");
            return Result<List<ShiftDto>>.ErrorResult($"Error fetching shifts: {ex.Message}");
        }
    }

    public async Task<Result> CreateShiftAsync(ShiftDto shift)
    {
        try
        {
            var createShiftDto = new CreateShiftDto
            {
                UserId = shift.UserId,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            };

            var response = await _httpClient.PostAsJsonAsync("/api/Shift", createShiftDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<object>>();
                if (result?.Success == true)
                {
                    return Result.SuccessResult();
                }
                return Result.ErrorResult(result?.Error ?? "Failed to create shift");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to create shift. Status: {response.StatusCode}, Error: {errorContent}");
            return Result.ErrorResult($"Failed to create shift: {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating shift: {ex.Message}");
            return Result.ErrorResult($"Error creating shift: {ex.Message}");
        }
    }

    public async Task<Result> DeleteShiftAsync(Guid shiftId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/Shift/{shiftId}");

            if (response.IsSuccessStatusCode)
            {
                return Result.SuccessResult();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to delete shift. Status: {response.StatusCode}, Error: {errorContent}");
            return Result.ErrorResult($"Failed to delete shift: {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting shift: {ex.Message}");
            return Result.ErrorResult($"Error deleting shift: {ex.Message}");
        }
    }
}