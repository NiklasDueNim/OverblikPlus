using Microsoft.EntityFrameworkCore;
using TaskMicroService.Common;
using TaskMicroService.DataAccess;
using TaskMicroService.dtos.Activity;
using TaskMicroService.Entities;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class ActivityService : IActivityService
{
    private readonly TaskDbContext _dbContext;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(TaskDbContext dbContext, ILogger<ActivityService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ReadActivityDto>>> GetAllActivitiesAsync()
    {
        try
        {
            var activities = await _dbContext.Activities
                .OrderBy(a => a.StartDateTime)
                .ToListAsync();

            var activityDtos = activities.Select(MapToReadDto).ToList();
            return Result<IEnumerable<ReadActivityDto>>.SuccessResult(activityDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all activities");
            return Result<IEnumerable<ReadActivityDto>>.ErrorResult("An error occurred while retrieving activities.");
        }
    }

    public async Task<Result<IEnumerable<ReadActivityDto>>> GetActivitiesForDateAsync(DateTime date)
    {
        try
        {
            var activities = await _dbContext.Activities
                .Where(a => a.StartDateTime.Date == date.Date)
                .OrderBy(a => a.StartDateTime)
                .ToListAsync();

            var activityDtos = activities.Select(MapToReadDto).ToList();
            return Result<IEnumerable<ReadActivityDto>>.SuccessResult(activityDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for date {Date}", date);
            return Result<IEnumerable<ReadActivityDto>>.ErrorResult("An error occurred while retrieving activities for the specified date.");
        }
    }

    public async Task<Result<IEnumerable<ReadActivityDto>>> GetActivitiesForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var activities = await _dbContext.Activities
                .Where(a => a.StartDateTime.Date >= startDate.Date && a.StartDateTime.Date <= endDate.Date)
                .OrderBy(a => a.StartDateTime)
                .ToListAsync();

            var activityDtos = activities.Select(MapToReadDto).ToList();
            return Result<IEnumerable<ReadActivityDto>>.SuccessResult(activityDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for date range {StartDate} to {EndDate}", startDate, endDate);
            return Result<IEnumerable<ReadActivityDto>>.ErrorResult("An error occurred while retrieving activities for the specified date range.");
        }
    }

    public async Task<Result<ReadActivityDto>> GetActivityByIdAsync(Guid id)
    {
        try
        {
            var activity = await _dbContext.Activities.FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null)
            {
                return Result<ReadActivityDto>.ErrorResult($"Activity with ID {id} not found.");
            }

            var activityDto = MapToReadDto(activity);
            return Result<ReadActivityDto>.SuccessResult(activityDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity by ID {Id}", id);
            return Result<ReadActivityDto>.ErrorResult("An error occurred while retrieving the activity.");
        }
    }

    public async Task<Result<Guid>> CreateActivityAsync(CreateActivityDto createActivityDto)
    {
        try
        {
            var activityEntity = new ActivityEntity
            {
                Id = Guid.NewGuid(),
                Title = createActivityDto.Title,
                Description = createActivityDto.Description,
                StartDateTime = createActivityDto.StartDateTime,
                EndDateTime = createActivityDto.EndDateTime,
                ResponsibleStaff = System.Text.Json.JsonSerializer.Serialize(createActivityDto.ResponsibleStaff),
                ActivityType = createActivityDto.ActivityType,
                Location = createActivityDto.Location,
                MaxParticipants = createActivityDto.MaxParticipants,
                RequiresAssistance = createActivityDto.RequiresAssistance,
                IsWheelchairAccessible = createActivityDto.IsWheelchairAccessible,
                SpecialRequirements = createActivityDto.SpecialRequirements,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createActivityDto.CreatedBy,
                Participants = "[]" // Empty JSON array
            };

            _dbContext.Activities.Add(activityEntity);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Activity created successfully with ID {Id}", activityEntity.Id);
            return Result<Guid>.SuccessResult(activityEntity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating activity");
            return Result<Guid>.ErrorResult("An error occurred while creating the activity.");
        }
    }

    public async Task<Result> UpdateActivityAsync(Guid id, CreateActivityDto updateActivityDto)
    {
        try
        {
            var activity = await _dbContext.Activities.FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null)
            {
                return Result.ErrorResult($"Activity with ID {id} not found.");
            }

            activity.Title = updateActivityDto.Title;
            activity.Description = updateActivityDto.Description;
            activity.StartDateTime = updateActivityDto.StartDateTime;
            activity.EndDateTime = updateActivityDto.EndDateTime;
            activity.ResponsibleStaff = System.Text.Json.JsonSerializer.Serialize(updateActivityDto.ResponsibleStaff);
            activity.ActivityType = updateActivityDto.ActivityType;
            activity.Location = updateActivityDto.Location;
            activity.MaxParticipants = updateActivityDto.MaxParticipants;
            activity.RequiresAssistance = updateActivityDto.RequiresAssistance;
            activity.IsWheelchairAccessible = updateActivityDto.IsWheelchairAccessible;
            activity.SpecialRequirements = updateActivityDto.SpecialRequirements;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Activity updated successfully with ID {Id}", id);
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity {Id}", id);
            return Result.ErrorResult("An error occurred while updating the activity.");
        }
    }

    public async Task<Result> DeleteActivityAsync(Guid id)
    {
        try
        {
            var activity = await _dbContext.Activities.FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null)
            {
                return Result.ErrorResult($"Activity with ID {id} not found.");
            }

            _dbContext.Activities.Remove(activity);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Activity deleted successfully with ID {Id}", id);
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity {Id}", id);
            return Result.ErrorResult("An error occurred while deleting the activity.");
        }
    }

    public async Task<Result> JoinActivityAsync(Guid activityId, Guid userId)
    {
        try
        {
            var activity = await _dbContext.Activities.FirstOrDefaultAsync(a => a.Id == activityId);
            if (activity == null)
            {
                return Result.ErrorResult($"Activity with ID {activityId} not found.");
            }

            var participants = ParseParticipants(activity.Participants);
            
            if (participants.Contains(userId))
            {
                return Result.ErrorResult("User is already participating in this activity.");
            }

            if (participants.Count >= activity.MaxParticipants)
            {
                return Result.ErrorResult("Activity is full.");
            }

            participants.Add(userId);
            activity.Participants = System.Text.Json.JsonSerializer.Serialize(participants);
            
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {UserId} joined activity {ActivityId}", userId, activityId);
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining activity {ActivityId} for user {UserId}", activityId, userId);
            return Result.ErrorResult("An error occurred while joining the activity.");
        }
    }

    public async Task<Result> LeaveActivityAsync(Guid activityId, Guid userId)
    {
        try
        {
            var activity = await _dbContext.Activities.FirstOrDefaultAsync(a => a.Id == activityId);
            if (activity == null)
            {
                return Result.ErrorResult($"Activity with ID {activityId} not found.");
            }

            var participants = ParseParticipants(activity.Participants);
            
            if (!participants.Contains(userId))
            {
                return Result.ErrorResult("User is not participating in this activity.");
            }

            participants.Remove(userId);
            activity.Participants = System.Text.Json.JsonSerializer.Serialize(participants);
            
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {UserId} left activity {ActivityId}", userId, activityId);
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving activity {ActivityId} for user {UserId}", activityId, userId);
            return Result.ErrorResult("An error occurred while leaving the activity.");
        }
    }

    public async Task<Result<bool>> CanUserJoinActivityAsync(Guid activityId, Guid userId)
    {
        try
        {
            var activity = await _dbContext.Activities.FirstOrDefaultAsync(a => a.Id == activityId);
            if (activity == null)
            {
                return Result<bool>.ErrorResult($"Activity with ID {activityId} not found.");
            }

            var participants = ParseParticipants(activity.Participants);
            
            // Check if user is already participating
            if (participants.Contains(userId))
            {
                return Result<bool>.SuccessResult(false);
            }

            // Check if activity is full
            if (participants.Count >= activity.MaxParticipants)
            {
                return Result<bool>.SuccessResult(false);
            }

            // Check if activity is in the future
            if (activity.StartDateTime <= DateTime.Now)
            {
                return Result<bool>.SuccessResult(false);
            }

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can join activity {ActivityId}", userId, activityId);
            return Result<bool>.ErrorResult("An error occurred while checking activity eligibility.");
        }
    }

    private ReadActivityDto MapToReadDto(ActivityEntity entity)
    {
        return new ReadActivityDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            StartDateTime = entity.StartDateTime,
            EndDateTime = entity.EndDateTime,
            Participants = ParseParticipants(entity.Participants),
            ResponsibleStaff = ParseParticipants(entity.ResponsibleStaff),
            ActivityType = entity.ActivityType,
            Location = entity.Location,
            MaxParticipants = entity.MaxParticipants,
            RequiresAssistance = entity.RequiresAssistance,
            IsWheelchairAccessible = entity.IsWheelchairAccessible,
            SpecialRequirements = entity.SpecialRequirements,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        };
    }

    private List<Guid> ParseParticipants(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            return new List<Guid>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(jsonString) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }
}
