using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- HARDCODED ENVIRONMENT ---
var environment = "dev"; // Skift til "prod" for produktionsmiljø

string taskApiBaseUrl = "https://overblikplus-task-api-dev-aqcja5a8htcwb8fp.westeurope-01.azurewebsites.net";
string userApiBaseUrl = "https://overblikplus-user-api-dev-cheeh0a0fgc0ayh5.westeurope-01.azurewebsites.net";

if (environment == "prod")
{
    taskApiBaseUrl = "https://overblikplus-task-api-prod.azurewebsites.net";
    userApiBaseUrl = "https://overblikplus-user-api-prod.azurewebsites.net";
}

// Log URL'er for fejlfinding
Console.WriteLine($"Environment: {environment}");
Console.WriteLine($"Task API Base URL: {taskApiBaseUrl}");
Console.WriteLine($"User API Base URL: {userApiBaseUrl}");

// --- AUTHENTICATION OG JWT CONFIGURATION ---
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

// --- HTTP CLIENTS TIL API SERVICES ---
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri(userApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<ITaskService, TaskService>(client =>
{
    client.BaseAddress = new Uri(taskApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(userApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<ITaskStepService, TaskStepService>(client =>
{
    client.BaseAddress = new Uri(taskApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// Automapper og Authorization
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAuthorizationCore();

// --- START APP ---
await builder.Build().RunAsync();
