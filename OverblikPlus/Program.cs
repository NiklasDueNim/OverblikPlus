using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Hent miljøvariabler fra runtime
var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "dev";

string taskApiBaseUrl;
string userApiBaseUrl;

if (environment == "prod")
{
    // Prod miljøvariabler
    taskApiBaseUrl = Environment.GetEnvironmentVariable("TASK_API_BASE_URL_PROD") ?? "https://default-prod-task-api.com";
    userApiBaseUrl = Environment.GetEnvironmentVariable("USER_API_BASE_URL_PROD") ?? "https://default-prod-user-api.com";
}
else
{
    // Dev miljøvariabler
    taskApiBaseUrl = Environment.GetEnvironmentVariable("TASK_API_BASE_URL_DEV") ?? "http://localhost:5101";
    userApiBaseUrl = Environment.GetEnvironmentVariable("USER_API_BASE_URL_DEV") ?? "http://localhost:5102";
}

// Log URL'erne til fejlfindingsformål (fjernes i prod)
Console.WriteLine($"Environment: {environment}");
Console.WriteLine($"Task API Base URL: {taskApiBaseUrl}");
Console.WriteLine($"User API Base URL: {userApiBaseUrl}");

// --- AUTHENTICATION OG JWT CONFIGURATION ---
// Tilføj authentication state provider
builder.Services.AddSingleton<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());

// JWT authorization med dynamiske URL'er
builder.Services.AddScoped<JwtAuthorizationMessageHandler>(provider =>
    new JwtAuthorizationMessageHandler(provider.GetRequiredService<CustomAuthStateProvider>())
        .ConfigureHandler(authorizedUrls: new[]
        {
            taskApiBaseUrl,
            userApiBaseUrl
        }));

// --- HTTP CLIENTS TIL API SERVICES ---
// UserService
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri(userApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// TaskService
builder.Services.AddHttpClient<ITaskService, TaskService>(client =>
{
    client.BaseAddress = new Uri(taskApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// TaskStepService
builder.Services.AddHttpClient<ITaskStepService, TaskStepService>(client =>
{
    client.BaseAddress = new Uri(taskApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// AuthService
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(userApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// --- AUTOMAPPER OG AUTHORIZATION ---
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAuthorizationCore();

// --- START APPEN ---
await builder.Build().RunAsync();
