using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Helpers;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.dtos.Activity;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly ILoggerService _logger;

    public ActivityService(IActivityRepository activityRepository, ILoggerService logger)
    {
        _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<ReadActivityDto>>> GetAllActivitiesAsync()
    {
        try
        {
            var activities = await _activityRepository.GetAllActivitiesAsync();

            var activityDtos = activities.Select(MapToReadDto).ToList();
            return Result<IEnumerable<ReadActivityDto>>.SuccessResult(activityDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting all activities", ex);
            return Result<IEnumerable<ReadActivityDto>>.ErrorResult("An error occurred while retrieving activities.");
        }
    }

    public async Task<Result<IEnumerable<ReadActivityDto>>> GetActivitiesForDateAsync(DateTime date)
    {
        try
        {
            var activities = await _activityRepository.GetActivitiesForDateAsync(date);

            var activityDtos = activities.Select(MapToReadDto).ToList();
            return Result<IEnumerable<ReadActivityDto>>.SuccessResult(activityDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting activities for date {date}", ex);
            return Result<IEnumerable<ReadActivityDto>>.ErrorResult("An error occurred while retrieving activities for the specified date.");
        }
    }

    public async Task<Result<IEnumerable<ReadActivityDto>>> GetActivitiesForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var activities = await _activityRepository.GetActivitiesForDateRangeAsync(startDate, endDate);

            var activityDtos = activities.Select(MapToReadDto).ToList();
            return Result<IEnumerable<ReadActivityDto>>.SuccessResult(activityDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting activities for date range {startDate} to {endDate}", ex);
            return Result<IEnumerable<ReadActivityDto>>.ErrorResult("An error occurred while retrieving activities for the specified date range.");
        }
    }

    public async Task<Result<ReadActivityDto>> GetActivityByIdAsync(Guid id)
    {
        try
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity == null)
            {
                return Result<ReadActivityDto>.ErrorResult($"Activity with ID {id} not found.");
            }

            var activityDto = MapToReadDto(activity);
            return Result<ReadActivityDto>.SuccessResult(activityDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting activity by ID {id}", ex);
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
                ResponsibleStaff = JsonHelper.Serialize(createActivityDto.ResponsibleStaff),
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

            await _activityRepository.AddAsync(activityEntity);
            await _activityRepository.SaveChangesAsync();

            _logger.LogInfo($"Activity created successfully with ID {activityEntity.Id}");
            return Result<Guid>.SuccessResult(activityEntity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating activity", ex);
            return Result<Guid>.ErrorResult("An error occurred while creating the activity.");
        }
    }

    public async Task<Result> UpdateActivityAsync(Guid id, CreateActivityDto updateActivityDto)
    {
        try
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity == null)
            {
                return Result.ErrorResult($"Activity with ID {id} not found.");
            }

            activity.Title = updateActivityDto.Title;
            activity.Description = updateActivityDto.Description;
            activity.StartDateTime = updateActivityDto.StartDateTime;
            activity.EndDateTime = updateActivityDto.EndDateTime;
            activity.ResponsibleStaff = JsonHelper.Serialize(updateActivityDto.ResponsibleStaff);
            activity.ActivityType = updateActivityDto.ActivityType;
            activity.Location = updateActivityDto.Location;
            activity.MaxParticipants = updateActivityDto.MaxParticipants;
            activity.RequiresAssistance = updateActivityDto.RequiresAssistance;
            activity.IsWheelchairAccessible = updateActivityDto.IsWheelchairAccessible;
            activity.SpecialRequirements = updateActivityDto.SpecialRequirements;

            await _activityRepository.UpdateAsync(activity);
            await _activityRepository.SaveChangesAsync();

            _logger.LogInfo($"Activity updated successfully with ID {id}");
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating activity {id}", ex);
            return Result.ErrorResult("An error occurred while updating the activity.");
        }
    }

    public async Task<Result> DeleteActivityAsync(Guid id)
    {
        try
        {
            var activity = await _activityRepository.GetByIdAsync(id);
            if (activity == null)
            {
                return Result.ErrorResult($"Activity with ID {id} not found.");
            }

            await _activityRepository.DeleteAsync(activity);
            await _activityRepository.SaveChangesAsync();

            _logger.LogInfo($"Activity deleted successfully with ID {id}");
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting activity {id}", ex);
            return Result.ErrorResult("An error occurred while deleting the activity.");
        }
    }

    public async Task<Result> JoinActivityAsync(Guid activityId, Guid userId)
    {
        try
        {
            var activity = await _activityRepository.GetByIdAsync(activityId);
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
            activity.Participants = JsonHelper.Serialize(participants);
            
            await _activityRepository.UpdateAsync(activity);
            await _activityRepository.SaveChangesAsync();

            _logger.LogInfo($"User {userId} joined activity {activityId}");
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error joining activity {activityId} for user {userId}", ex);
            return Result.ErrorResult("An error occurred while joining the activity.");
        }
    }

    public async Task<Result> LeaveActivityAsync(Guid activityId, Guid userId)
    {
        try
        {
            var activity = await _activityRepository.GetByIdAsync(activityId);
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
            activity.Participants = JsonHelper.Serialize(participants);
            
            await _activityRepository.UpdateAsync(activity);
            await _activityRepository.SaveChangesAsync();

            _logger.LogInfo($"User {userId} left activity {activityId}");
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error leaving activity {activityId} for user {userId}", ex);
            return Result.ErrorResult("An error occurred while leaving the activity.");
        }
    }

    public async Task<Result<bool>> CanUserJoinActivityAsync(Guid activityId, Guid userId)
    {
        try
        {
            var activity = await _activityRepository.GetByIdAsync(activityId);
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
            _logger.LogError($"Error checking if user {userId} can join activity {activityId}", ex);
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
        return JsonHelper.Deserialize<List<Guid>>(jsonString) ?? new List<Guid>();
    }
}
