using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.dtos.Facility;
using TaskMicroService.Entities;
using TaskMicroService.Repositories.Interfaces;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _repository;
    private readonly IImageService _imageService;
    private readonly ILoggerService _logger;

    public FacilityService(IFacilityRepository repository, IImageService imageService, ILoggerService logger)
    {
        _repository = repository;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ReadFacilityDto>>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return Result<IEnumerable<ReadFacilityDto>>.SuccessResult(list.Select(Map));
    }

    public async Task<Result<ReadFacilityDto>> GetByIdAsync(Guid id)
    {
        var f = await _repository.GetByIdAsync(id);
        return f == null
            ? Result<ReadFacilityDto>.ErrorResult("Facility not found.")
            : Result<ReadFacilityDto>.SuccessResult(Map(f));
    }

    public async Task<Result<Guid>> CreateAsync(CreateFacilityDto dto)
    {
        try
        {
            string? imageUrl = null;
            if (!string.IsNullOrEmpty(dto.ImageBase64))
                imageUrl = await _imageService.UploadImageAsync(dto.ImageBase64);

            var entity = new FacilityEntity
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = imageUrl,
                ResponsibleStaffId = dto.ResponsibleStaffId,
                BostedId = dto.BostedId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            return Result<Guid>.SuccessResult(entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating facility: {ex.Message}", ex);
            return Result<Guid>.ErrorResult("Could not create facility.");
        }
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateFacilityDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return Result.ErrorResult("Facility not found.");

        try
        {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.ResponsibleStaffId = dto.ResponsibleStaffId;

            if (!string.IsNullOrEmpty(dto.ImageBase64))
                entity.ImageUrl = await _imageService.UploadImageAsync(dto.ImageBase64);
            else if (dto.ImageUrl != null)
                entity.ImageUrl = dto.ImageUrl;

            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();
            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating facility: {ex.Message}", ex);
            return Result.ErrorResult("Could not update facility.");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return Result.ErrorResult("Facility not found.");

        await _repository.DeleteAsync(entity);
        await _repository.SaveChangesAsync();
        return Result.SuccessResult();
    }

    private static ReadFacilityDto Map(FacilityEntity f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        Description = f.Description,
        ImageUrl = f.ImageUrl,
        ResponsibleStaffId = f.ResponsibleStaffId
    };
}
