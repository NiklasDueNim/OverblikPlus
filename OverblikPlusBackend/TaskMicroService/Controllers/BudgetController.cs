using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OverblikPlus.Shared.Interfaces;
using System.Security.Claims;
using TaskMicroService.Dtos.Budget;
using TaskMicroService.Services.Interfaces;


namespace TaskMicroService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    private readonly ILoggerService _logger;
    private readonly IBlobStorageService _blobStorageService;

    public BudgetController(IBudgetService budgetService, ILoggerService logger, IBlobStorageService blobStorageService)
    {
        _budgetService = budgetService ?? throw new ArgumentNullException(nameof(budgetService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBudgets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in token");
            return Unauthorized();
        }

        _logger.LogInfo($"Getting all budgets for user {userId}");
        var result = await _budgetService.GetAllBudgetsAsync(userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBudgetById(Guid id)
    {
        _logger.LogInfo($"Getting budget with id {id}");
        var result = await _budgetService.GetBudgetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (result.Data.UserId != userId)
        {
            return Forbid();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto createBudgetDto)
    {
        if (createBudgetDto == null)
        {
            return BadRequest("Budget data is required");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        createBudgetDto.UserId = userId;

        _logger.LogInfo($"Creating budget for user {userId}");
        var result = await _budgetService.CreateBudgetAsync(createBudgetDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetBudgetById), new { id = result.Data.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] UpdateBudgetDto updateBudgetDto)
    {
        if (updateBudgetDto == null)
        {
            return BadRequest("Budget data is required");
        }

        var existingResult = await _budgetService.GetBudgetByIdAsync(id);
        if (!existingResult.Success)
        {
            return NotFound(existingResult);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (existingResult.Data.UserId != userId)
        {
            return Forbid();
        }

        _logger.LogInfo($"Updating budget with id {id}");
        var result = await _budgetService.UpdateBudgetAsync(id, updateBudgetDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(Guid id)
    {
        var existingResult = await _budgetService.GetBudgetByIdAsync(id);
        if (!existingResult.Success)
        {
            return NotFound(existingResult);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (existingResult.Data.UserId != userId)
        {
            return Forbid();
        }

        _logger.LogInfo($"Deleting budget with id {id}");
        var result = await _budgetService.DeleteBudgetAsync(id);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadVoucher(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        try
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            using var stream = file.OpenReadStream();
            var fileUrl = await _blobStorageService.UploadImageAsync(stream, fileName);

            _logger.LogInfo($"File uploaded successfully: {fileUrl}");
            return Ok(new { url = fileUrl, fileName = fileName });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading file: {ex.Message}", ex);
            return StatusCode(500, "Error uploading file");
        }
    }
}
