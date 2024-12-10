namespace TaskMicroService.Services;
using Azure.Storage.Blobs;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "images";

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(imageStream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        
        await blobClient.DeleteIfExistsAsync();
    }
}