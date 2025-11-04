using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using TaskMicroService.Services.Interfaces;

namespace TaskMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ImageController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<ImageController> _logger;

        public ImageController(BlobServiceClient blobServiceClient, ILogger<ImageController> logger)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{*fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("images");
                
                if (!await containerClient.ExistsAsync())
                {
                    _logger.LogWarning($"Container 'images' does not exist");
                    return NotFound();
                }

                var blobClient = containerClient.GetBlobClient(fileName);
                
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning($"Blob '{fileName}' does not exist");
                    return NotFound();
                }

                var blobDownloadInfo = await blobClient.DownloadAsync();
                
                return File(blobDownloadInfo.Value.Content, blobDownloadInfo.Value.ContentType ?? "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving image: {fileName}");
                return StatusCode(500, "Error retrieving image");
            }
        }
    }
}
