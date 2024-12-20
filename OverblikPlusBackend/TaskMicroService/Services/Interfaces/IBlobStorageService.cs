namespace TaskMicroService.Services.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);

    Task DeleteImageAsync(string fileName);
}