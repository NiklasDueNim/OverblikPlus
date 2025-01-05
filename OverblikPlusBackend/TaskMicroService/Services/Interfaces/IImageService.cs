namespace TaskMicroService.Services.Interfaces;

public interface IImageService
{
    Task<string> UploadImageAsync(string imageBase64);
    
    Task DeleteImageAsync(string imageUrl);
}