using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using TaskMicroService.Dtos.Calendar;

namespace TaskMicroService.Test.IntegrationTests
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProgramTests()
        {
            var appFactory = new CustomWebApplicationFactory<Program>();
            _client = appFactory.CreateClient();
        }

        [Fact]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/CalendarEvent/1");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Swagger_EndpointReturnsSuccess()
        {
            // Arrange
            var url = "/swagger/index.html";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}