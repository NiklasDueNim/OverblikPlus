using Azure.Storage.Blobs;
using TaskMicroService.Services.Interfaces;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "images";
    private readonly string _blobBaseUrl;

    public BlobStorageService(BlobServiceClient blobServiceClient, string blobBaseUrl)
    {
        _blobServiceClient = blobServiceClient;
        _blobBaseUrl = blobBaseUrl;
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(imageStream, overwrite: true);

        return $"{_blobBaseUrl}/{fileName}";
    }

    public async Task DeleteImageAsync(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        
        await blobClient.DeleteIfExistsAsync();
    }
}