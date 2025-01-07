using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Services;

public class ImageService : IImageService
{
    private readonly IBlobStorageService _blobStorageService;

    public ImageService(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    }

    public async Task<string> UploadImageAsync(string imageBase64)
    {
        
        var imageBytes = Convert.FromBase64String(imageBase64);
        using var stream = new MemoryStream(imageBytes);
        var blobFileName = $"{Guid.NewGuid()}.jpg";
    
        var imageUrl = await _blobStorageService.UploadImageAsync(stream, blobFileName);
        
        return imageUrl;
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        var blobFileName = imageUrl.Substring(imageUrl.LastIndexOf('/') + 1);
        await _blobStorageService.DeleteImageAsync(blobFileName);
    }
}
