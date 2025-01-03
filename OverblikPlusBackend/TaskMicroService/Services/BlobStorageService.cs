using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TaskMicroService.Services.Interfaces;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "images";
    private readonly string _blobBaseUrl;
    
    public BlobStorageService(BlobServiceClient blobServiceClient, string blobBaseUrl)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        _blobBaseUrl = blobBaseUrl ?? throw new ArgumentNullException(nameof(blobBaseUrl));
    }
    
    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        if (imageStream == null) 
            throw new ArgumentNullException(nameof(imageStream));
        
        if (string.IsNullOrEmpty(fileName)) 
            throw new ArgumentNullException(nameof(fileName));

        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(imageStream, overwrite: true);
        
        return $"{_blobBaseUrl}/{ContainerName}/{fileName}";
    }

    
    public async Task DeleteImageAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) 
            throw new ArgumentNullException(nameof(fileName));
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        
        await blobClient.DeleteIfExistsAsync();
    }
}