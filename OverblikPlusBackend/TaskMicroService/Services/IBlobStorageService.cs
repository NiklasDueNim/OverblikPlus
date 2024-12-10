namespace TaskMicroService.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);

    Task DeleteImageAsync(string fileName);
}