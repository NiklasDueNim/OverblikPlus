using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- HENT MILJØVARIABLER ELLER DEFAULT VÆRDIER ---
// Bestem API-base-URL'er afhængigt af miljø
var taskApiBaseUrl = Environment.GetEnvironmentVariable("TASK_API_BASE_URL") ?? "http://localhost:5101";
var userApiBaseUrl = "overblikplus-user-api-dev-cheeh0a0fgc0ayh5.westeurope-01.azurewebsites.net" ?? "http://localhost:5102";


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
