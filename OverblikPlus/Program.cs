using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using OverblikPlus.AuthHelpers;
using OverblikPlus.Profiles;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Load base configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

// Load environment-specific configuration
var environment = builder.HostEnvironment.Environment;
Console.WriteLine($"Host Environment: {environment}");

// For Blazor WebAssembly, we need to manually load environment-specific config
if (environment == "Development")
{
    builder.Configuration.AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: false);
}
else if (environment == "Production")
{
    builder.Configuration.AddJsonFile("appsettings.production.json", optional: true, reloadOnChange: false);
}

var configuration = builder.Configuration;
var envConfig = configuration["ENVIRONMENT"] ?? "dev";

// Determine environment based on where the app is running
var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
var host = baseUri.Host;
var port = baseUri.Port;

// Debug logging
Console.WriteLine($"BaseAddress: {builder.HostEnvironment.BaseAddress}");
Console.WriteLine($"Host: {host}");
Console.WriteLine($"Port: {port}");

string taskApiBaseUrl;
string userApiBaseUrl;

// Check if running on production domain (overblikplus.dk)
if (host.Contains("overblikplus.dk"))
{
    // PRODUCTION: Use PROD Azure API endpoints
    taskApiBaseUrl = "https://overblikplus-task-api-prod.azurewebsites.net";
    userApiBaseUrl = "https://overblikplus-user-api-prod.azurewebsites.net";
    envConfig = "production";
}
// Check if running on production Azure Static Web App
else if (host.Contains("nice-wave-08dd97903.1.azurestaticapps.net"))
{
    // PRODUCTION Azure Static Web App (før custom domain er sat op)
    taskApiBaseUrl = "https://overblikplus-task-api-prod.azurewebsites.net";
    userApiBaseUrl = "https://overblikplus-user-api-prod.azurewebsites.net";
    envConfig = "production";
}
// Check if running on development Azure Static Web App
else if (host.Contains("witty-meadow-0c52c9003.2.azurestaticapps.net"))
{
    // DEVELOPMENT: Use DEV Azure API endpoints
    taskApiBaseUrl = "https://overblikplus-task-api-dev.azurewebsites.net";
    userApiBaseUrl = "https://overblikplus-user-api-dev.azurewebsites.net";
    envConfig = "development-azure";
}
// Fallback for other azurestaticapps.net URLs (shouldn't happen, but just in case)
else if (host.Contains("azurestaticapps.net"))
{
    // Unknown Azure Static Web App - default to dev for safety
    taskApiBaseUrl = "https://overblikplus-task-api-dev.azurewebsites.net";
    userApiBaseUrl = "https://overblikplus-user-api-dev.azurewebsites.net";
    envConfig = "development-azure";
}
else if (host == "localhost" || host == "127.0.0.1")
{
    // Local development: Check URL for backend preference
    // URL examples:
    // - http://localhost:5226 (default: Rider ports 5004/5002)
    // - http://localhost:5003 (Docker frontend: Docker ports 5001/5002)
    // - http://localhost:5226?backend=docker (Docker ports 5001/5002)
    // - http://localhost:5226?backend=azure (Azure cloud)
    
    var uri = new Uri(baseUri.ToString());
    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
    var backendType = query["backend"]?.ToLower();
    
    // Blazor WASM BaseAddress kan være uden port - tjek også window.location via JS interop
    // Men først: hvis port er -1 eller 0, prøv at få det fra BaseAddress string
    var actualPort = port;
    if (port == -1 || port == 0 || port == 80 || port == 443)
    {
        // Prøv at parse porten fra BaseAddress string
        var baseAddressStr = builder.HostEnvironment.BaseAddress;
        var portMatch = System.Text.RegularExpressions.Regex.Match(baseAddressStr, @":(\d+)");
        if (portMatch.Success && int.TryParse(portMatch.Groups[1].Value, out var parsedPort))
        {
            actualPort = parsedPort;
            Console.WriteLine($"Parsed port from BaseAddress: {actualPort}");
        }
    }
    
    Console.WriteLine($"Checking port: {port}, actualPort: {actualPort}, backendType: {backendType}");
    
    // Check environment variables first (fra docker-compose.yml)
    var envTaskUrl = configuration["TASK_API_BASE_URL"] ?? Environment.GetEnvironmentVariable("TaskService__BaseUrl");
    var envUserUrl = configuration["USER_API_BASE_URL"] ?? Environment.GetEnvironmentVariable("UserService__BaseUrl");
    
    if (!string.IsNullOrEmpty(envTaskUrl) && !string.IsNullOrEmpty(envUserUrl))
    {
        taskApiBaseUrl = envTaskUrl;
        userApiBaseUrl = envUserUrl;
        envConfig = "development-docker";
        Console.WriteLine($"Using environment variables: Task={taskApiBaseUrl}, User={userApiBaseUrl}");
    }
    // Hvis frontend kører på port 5003, så er det Docker frontend - brug Docker backend
    else if (actualPort == 5003 || backendType == "docker")
    {
        taskApiBaseUrl = "http://localhost:5002";  // Docker TaskService
        userApiBaseUrl = "http://localhost:5001";  // Docker UserService
        envConfig = "development-docker";
        Console.WriteLine("Using Docker backend (5001/5002) - detected from port");
    }
    else if (backendType == "azure")
    {
        taskApiBaseUrl = "https://overblikplus-task-api-dev-aqcja5a8htcwb8fp.westeurope-01.azurewebsites.net";
        userApiBaseUrl = "https://overblikplus-user-api-dev-cheeh0a0fgc0ayh5.westeurope-01.azurewebsites.net";
        envConfig = "development-azure";
    }
    else
    {
        // Default: Rider ports (UserService kører på 5004, TaskService på 5002)
        taskApiBaseUrl = "http://localhost:5002";
        userApiBaseUrl = "http://localhost:5004";  // Rider UserService
        envConfig = "development";
        Console.WriteLine("Using Rider backend (5004/5002)");
    }
}
else
{
    // Fallback: Read from appsettings.json or use dev URLs (prod not deployed yet)
    taskApiBaseUrl = configuration["TASK_API_BASE_URL"] ?? "https://overblikplus-task-api-dev.azurewebsites.net";
    userApiBaseUrl = configuration["USER_API_BASE_URL"] ?? "https://overblikplus-user-api-dev.azurewebsites.net";
}

Console.WriteLine($"Host Environment: {environment}");
Console.WriteLine($"Config Environment: {envConfig}");
Console.WriteLine($"TASK API Base URL: {taskApiBaseUrl}");
Console.WriteLine($"USER API Base URL: {userApiBaseUrl}");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ActivityNoticeService>();

builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<JwtAuthorizationMessageHandler>(provider =>
{
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<JwtAuthorizationMessageHandler>();
    return new JwtAuthorizationMessageHandler(
        provider.GetRequiredService<CustomAuthStateProvider>(),
        logger,
        new[] { taskApiBaseUrl, userApiBaseUrl });
});

void ConfigureHttpClient<TClient, TImplementation>(IServiceCollection services, string baseUrl)
    where TClient : class
    where TImplementation : class, TClient
{
    services.AddHttpClient<TClient, TImplementation>(client =>
    {
        client.BaseAddress = new Uri(baseUrl);
    }).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();
}

ConfigureHttpClient<IUserService, UserService>(builder.Services, userApiBaseUrl);
ConfigureHttpClient<ICalendarEventService, CalendarEventService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<ITaskService, TaskService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<IAuthService, AuthService>(builder.Services, userApiBaseUrl);
ConfigureHttpClient<ITaskStepService, TaskStepService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<IRelativeService, RelativeService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<IAnnouncementService, AnnouncementService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<IMoodService, MoodService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<IActivityService, ActivityService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<IFacilityService, FacilityService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<IShiftService, ShiftService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<INotificationService, NotificationService>(builder.Services, taskApiBaseUrl);
ConfigureHttpClient<IBudgetService, BudgetService>(builder.Services, taskApiBaseUrl);

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();