using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// Tjek om vi kører i et Azure-miljø (f.eks. dev eller prod)
var taskApiBaseUrl = Environment.GetEnvironmentVariable("TASK_API_BASE_URL");
var userApiBaseUrl = Environment.GetEnvironmentVariable("USER_API_BASE_URL");

// Fallback til lokal udvikling, hvis miljøvariabler ikke er sat
if (string.IsNullOrEmpty(taskApiBaseUrl) || string.IsNullOrEmpty(userApiBaseUrl))
{
    // Hent fra en lokal konfigurationsfil (kun til lokal udvikling)
    var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var config = await http.GetFromJsonAsync<Dictionary<string, string>>("appsettings.json");

    taskApiBaseUrl ??= config?["TaskApiBaseUrl"] ?? "http://localhost:5101";
    userApiBaseUrl ??= config?["UserApiBaseUrl"] ?? "http://localhost:5102";
}

// Log URL'erne til fejlfindingsformål (fjernes i prod)
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
