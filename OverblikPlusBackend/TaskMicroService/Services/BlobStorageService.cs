using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TaskMicroService.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _blobBaseUrl;
    private readonly IHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public BlobStorageService(
        BlobServiceClient blobServiceClient, 
        string blobBaseUrl,
        IHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        _blobBaseUrl = blobBaseUrl ?? throw new ArgumentNullException(nameof(blobBaseUrl));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        if (imageStream == null)
            throw new ArgumentNullException(nameof(imageStream));

        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));

        var containerClient = _blobServiceClient.GetBlobContainerClient("images");
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(imageStream, overwrite: true);

        // When images live in Azurite (local dev or the Coolify-hosted deployment),
        // the blob host (e.g. the docker-internal "azurite" host) is not reachable from
        // the browser. Return an API proxy URL so the client always hits a reachable
        // endpoint (task-api) and avoids CORS issues.
        if (IsLocalDevStorage(_blobBaseUrl))
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                var baseUrl = $"{request.Scheme}://{request.Host}";
                return $"{baseUrl}/api/Image/{fileName}";
            }
            // Fallback if HttpContext is not available
            return $"http://localhost:5002/api/Image/{fileName}";
        }

        // For production, use the configured blob base URL with container name
        // blobBaseUrl typically includes account name, so we need to add /images/ container
        var blobUrl = _blobBaseUrl.TrimEnd('/');
        if (!blobUrl.Contains("/images"))
        {
            blobUrl = $"{blobUrl}/images";
        }
        return $"{blobUrl}/{fileName}";
    }

    // Detects local/dev blob storage (Azurite) regardless of how the host is addressed:
    // "localhost:10000" (host run), "azurite" (docker-compose), 127.0.0.1 or the dev account.
    private static bool IsLocalDevStorage(string blobBaseUrl)
    {
        if (string.IsNullOrEmpty(blobBaseUrl))
            return false;

        return blobBaseUrl.Contains("localhost:10000")
            || blobBaseUrl.Contains("127.0.0.1:10000")
            || blobBaseUrl.Contains("azurite")
            || blobBaseUrl.Contains("devstoreaccount1");
    }

    public async Task DeleteImageAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));

        var containerClient = _blobServiceClient.GetBlobContainerClient("images");
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync();
    }
}