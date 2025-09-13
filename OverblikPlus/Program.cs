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
var taskApiBaseUrl = configuration["TASK_API_BASE_URL"];
var userApiBaseUrl = configuration["USER_API_BASE_URL"];

// Override with localhost URLs if running in Development mode
if (environment == "Development")
{
    taskApiBaseUrl = "http://localhost:5101";
    userApiBaseUrl = "http://localhost:5102";
}
else
{
    // Check if running on production domain
    var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
    var host = baseUri.Host;
    
    if (host.Contains("overblikplus.dk") || host.Contains("azurestaticapps.net"))
    {
        // Use production Azure API endpoints
        taskApiBaseUrl = "https://overblikplus-task-api-dev-aqcja5a8htcwb8fp.westeurope-01.azurewebsites.net";
        userApiBaseUrl = "https://overblikplus-user-api-dev-cheeh0a0fgc0ayh5.westeurope-01.azurewebsites.net";
        envConfig = "production";
    }
}

Console.WriteLine($"Host Environment: {environment}");
Console.WriteLine($"Config Environment: {envConfig}");
Console.WriteLine($"TASK API Base URL: {taskApiBaseUrl}");
Console.WriteLine($"USER API Base URL: {userApiBaseUrl}");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<JwtAuthorizationMessageHandler>(provider =>
    new JwtAuthorizationMessageHandler(provider.GetRequiredService<CustomAuthStateProvider>())
        .ConfigureHandler(new[] { taskApiBaseUrl, userApiBaseUrl }));

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

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();

// Trigger deployment
