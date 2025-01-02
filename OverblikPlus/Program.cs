using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

var configuration = builder.Configuration;
var environment = configuration["ENVIRONMENT"] ?? "dev";
var taskApiBaseUrl = configuration["TASK_API_BASE_URL"];
var userApiBaseUrl = configuration["USER_API_BASE_URL"];

Console.WriteLine($"Environment: {environment}");
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

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();