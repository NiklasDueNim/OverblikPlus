using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Hent miljø fra runtime
var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "dev";

// Hent API base URL'er fra miljøvariabler
string taskApiBaseUrl;
string userApiBaseUrl;

if (environment == "prod")
{
    taskApiBaseUrl = Environment.GetEnvironmentVariable("TASK_API_BASE_URL_PROD") ?? "https://default-prod-task-api.com";
    userApiBaseUrl = Environment.GetEnvironmentVariable("USER_API_BASE_URL_PROD") ?? "https://default-prod-user-api.com";
}
else
{
    taskApiBaseUrl = Environment.GetEnvironmentVariable("TASK_API_BASE_URL_DEV") ?? "http://localhost:5101";
    userApiBaseUrl = Environment.GetEnvironmentVariable("USER_API_BASE_URL_DEV") ?? "http://localhost:5102";
}

// Log for fejlfindingsformål
Console.WriteLine($"Environment: {environment}");
Console.WriteLine($"Task API Base URL: {taskApiBaseUrl}");
Console.WriteLine($"User API Base URL: {userApiBaseUrl}");

// --- Authentication og JWT ---
builder.Services.AddSingleton<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());

// JWT authorization
builder.Services.AddScoped<JwtAuthorizationMessageHandler>(provider =>
    new JwtAuthorizationMessageHandler(provider.GetRequiredService<CustomAuthStateProvider>())
        .ConfigureHandler(authorizedUrls: new[]
        {
            taskApiBaseUrl,
            userApiBaseUrl
        }));

// --- HTTP Clients ---
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri(userApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<ITaskService, TaskService>(client =>
{
    client.BaseAddress = new Uri(taskApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// Tilføj AuthService
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(userApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// Automapper og Authorization
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAuthorizationCore();

// --- Start App ---
await builder.Build().RunAsync();
