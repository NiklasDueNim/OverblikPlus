using Azure.Storage.Blobs;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "images"; // Sørg for, at denne container er oprettet i Azure

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

    public async Task<Stream> DownloadImageAsync(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }
}