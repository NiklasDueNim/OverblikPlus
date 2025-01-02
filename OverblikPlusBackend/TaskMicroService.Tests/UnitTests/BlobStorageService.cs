using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;

public class BlobStorageServiceTests
{
    private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
    private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
    private readonly Mock<BlobClient> _mockBlobClient;
    private readonly BlobStorageService _blobStorageService;

    private readonly string _blobBaseUrl = "https://mockblobstorage.com";

    public BlobStorageServiceTests()
    {
        _mockBlobServiceClient = new Mock<BlobServiceClient>();
        _mockBlobContainerClient = new Mock<BlobContainerClient>();
        _mockBlobClient = new Mock<BlobClient>();

        _mockBlobServiceClient
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_mockBlobContainerClient.Object);

        _mockBlobContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);

        _blobStorageService = new BlobStorageService(_mockBlobServiceClient.Object, _blobBaseUrl);
    }

    [Fact]
    public async Task UploadImageAsync_UploadsFileAndReturnsUrl()
    {
        // Arrange
        var stream = new MemoryStream();
        var fileName = "test.jpg";

        _mockBlobContainerClient
            .Setup(x => x.CreateIfNotExistsAsync(PublicAccessType.Blob, null, null, default))
            .ReturnsAsync(Response.FromValue(Mock.Of<BlobContainerInfo>(), Mock.Of<Response>()));

        _mockBlobClient
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), true, default))
            .ReturnsAsync(Response.FromValue(Mock.Of<BlobContentInfo>(), Mock.Of<Response>()));

        // Act
        var result = await _blobStorageService.UploadImageAsync(stream, fileName);

        // Assert
        Assert.Equal($"{_blobBaseUrl}/images/{fileName}", result);
        _mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, default), Times.Once);
    }


    [Fact]
    public async Task UploadImageAsync_ThrowsException_WhenStreamIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _blobStorageService.UploadImageAsync(null!, "test.jpg"));
    }

    [Fact]
    public async Task UploadImageAsync_ThrowsException_WhenFileNameIsEmpty()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _blobStorageService.UploadImageAsync(stream, ""));
    }


    [Fact]
    public async Task DeleteImageAsync_DeletesFile()
    {
        // Arrange
        var fileName = "test.jpg";

        var mockContainerClient = new Mock<BlobContainerClient>();
        var mockBlobClient = new Mock<BlobClient>();

        mockContainerClient
            .Setup(c => c.GetBlobClient(fileName))
            .Returns(mockBlobClient.Object);

        mockBlobClient
            .Setup(b => b.DeleteIfExistsAsync(
                It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        _mockBlobServiceClient
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        // Act
        await _blobStorageService.DeleteImageAsync(fileName);

        // Assert
        mockBlobClient.Verify(
            b => b.DeleteIfExistsAsync(
                It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task DeleteImageAsync_ThrowsException_WhenFileNameIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _blobStorageService.DeleteImageAsync(""));
    }
}
