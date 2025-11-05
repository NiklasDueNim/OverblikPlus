using AutoMapper;
using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Dtos.Mood;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class MoodService : IMoodService
{
    private readonly IMoodRepository _moodRepository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MoodService(IMoodRepository moodRepository, IMapper mapper, ILoggerService logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _moodRepository = moodRepository ?? throw new ArgumentNullException(nameof(moodRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<Result<ReadMoodDto>> CreateMood(CreateMoodDto createMoodDto)
    {
        _logger.LogInfo($"Creating mood for user {createMoodDto.UserId}");

        var today = createMoodDto.Date.Date;
        var existingMood = await _moodRepository.GetMoodByUserIdAndDateAsync(createMoodDto.UserId, today);

        if (existingMood != null)
        {
            _logger.LogInfo($"Updating existing mood for user {createMoodDto.UserId} on {today}");
            existingMood.Rating = createMoodDto.Rating;
            existingMood.Note = createMoodDto.Note;
            await _moodRepository.UpdateAsync(existingMood);
            await _moodRepository.SaveChangesAsync();

            var updatedMoodDto = _mapper.Map<ReadMoodDto>(existingMood);
            return Result<ReadMoodDto>.SuccessResult(updatedMoodDto);
        }

        var moodEntity = _mapper.Map<MoodEntity>(createMoodDto);
        moodEntity.Id = Guid.NewGuid();

        await _moodRepository.AddAsync(moodEntity);
        await _moodRepository.SaveChangesAsync();

        _logger.LogInfo($"Mood created successfully with ID {moodEntity.Id}");
        var moodDto = _mapper.Map<ReadMoodDto>(moodEntity);
        return Result<ReadMoodDto>.SuccessResult(moodDto);
    }

    public async Task<Result<List<ReadMoodDto>>> GetMoodsForUserAsync(string userId, DateTime fromDate, DateTime toDate)
    {
        _logger.LogInfo($"Getting moods for user {userId} from {fromDate.Date} to {toDate.Date}");

        var moods = await _moodRepository.GetMoodsForUserAsync(userId, fromDate, toDate);
        var moodDtos = _mapper.Map<List<ReadMoodDto>>(moods);
        return Result<List<ReadMoodDto>>.SuccessResult(moodDtos);
    }

    public async Task<Result<List<ReadMoodWithUserDto>>> GetMoodsForBostedAsync(int bostedId, DateTime fromDate, DateTime toDate)
    {
        _logger.LogInfo($"Getting moods for bosted {bostedId} from {fromDate.Date} to {toDate.Date}");

        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var authHeader = httpContext?.Request.Headers["Authorization"].ToString() ?? string.Empty;
            var token = !string.IsNullOrEmpty(authHeader) ? authHeader.Replace("Bearer ", "").Trim() : null;
            
            var userApiClient = _httpClientFactory.CreateClient("UserApi");
            if (!string.IsNullOrEmpty(token))
            {
                userApiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("No Authorization token found in request headers when calling UserMicroService");
            }

            var usersResponse = await userApiClient.GetAsync($"/api/User/bosted/{bostedId}");
            if (!usersResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to get users for bosted {bostedId}: {usersResponse.StatusCode}");
                return Result<List<ReadMoodWithUserDto>>.ErrorResult($"Failed to get users for bosted: {usersResponse.StatusCode}");
            }

            var usersResult = await usersResponse.Content.ReadFromJsonAsync<Result<List<UserDto>>>();
            if (usersResult == null || !usersResult.Success || usersResult.Data == null || !usersResult.Data.Any())
            {
                _logger.LogInfo($"No users found for bosted {bostedId}");
                return Result<List<ReadMoodWithUserDto>>.SuccessResult(new List<ReadMoodWithUserDto>());
            }

            var userIds = usersResult.Data.Select(u => u.Id).ToList();

            var moods = await _moodRepository.GetMoodsForUsersAsync(userIds, fromDate, toDate);

            // Create a dictionary for quick user lookup
            var userDict = usersResult.Data.ToDictionary(u => u.Id);

            // Map moods with user information
            var moodsWithUsers = moods.Select(m => new ReadMoodWithUserDto
            {
                Id = m.Id,
                UserId = m.UserId,
                UserFirstName = userDict.TryGetValue(m.UserId, out var user) ? user.FirstName : "Unknown",
                UserLastName = userDict.TryGetValue(m.UserId, out var user2) ? user2.LastName : "Unknown",
                Date = m.Date,
                Rating = m.Rating,
                Note = m.Note
            }).ToList();

            _logger.LogInfo($"Found {moodsWithUsers.Count} moods for bosted {bostedId}");
            return Result<List<ReadMoodWithUserDto>>.SuccessResult(moodsWithUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting moods for bosted {bostedId}: {ex.Message}", ex);
            return Result<List<ReadMoodWithUserDto>>.ErrorResult($"Error getting moods for bosted: {ex.Message}");
        }
    }
}
