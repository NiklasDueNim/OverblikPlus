using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskMicroService.DataAccess;
using TaskMicroService.Services.Interfaces;
using Xunit;

namespace TaskMicroService.Test.IntegrationTests
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task DatabaseConnection_IsSuccessful()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var canConnect = await dbContext.Database.CanConnectAsync();
            Assert.True(canConnect);
        }

        // [Fact]
        // public async Task JwtConfiguration_IsValid()
        // {
        //     var response = await _client.GetAsync("/api/Task");
        //     Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        // }

        // [Fact]
        // public async Task BlobStorageConfiguration_IsValid()
        // {
        //     using var scope = _factory.Services.CreateScope();
        //     var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
        //     var container = blobServiceClient.GetBlobContainerClient("images");
        //     var exists = await container.ExistsAsync();
        //     Assert.True(exists);
        // }

        // [Fact]
        // public async Task CorsConfiguration_IsValid()
        // {
        //     var request = new HttpRequestMessage(HttpMethod.Options, "/api/CalendarEvent/1");
        //     request.Headers.Add("Origin", "http://example.com");
        //     request.Headers.Add("Access-Control-Request-Method", "GET");
        //
        //     var response = await _client.SendAsync(request);
        //     Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        //     Assert.Contains("Access-Control-Allow-Origin", response.Headers.ToString());
        // }

        [Fact]
        public void DependencyInjection_IsConfiguredCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            Assert.NotNull(taskService);
        }

        // [Fact]
        // public async Task Middleware_LogsRequestsAndResponses()
        // {
        //     var response = await _client.GetAsync("/api/CalendarEvent/1");
        //     Assert.True(response.IsSuccessStatusCode);
        //     // Check logs for request and response logging
        // }
        //
        // [Fact]
        // public async Task Swagger_IsAvailable()
        // {
        //     var response = await _client.GetAsync("/swagger/index.html");
        //     response.EnsureSuccessStatusCode();
        //     var content = await response.Content.ReadAsStringAsync();
        //     Assert.Contains("Swagger UI", content);
        // }
    }
}