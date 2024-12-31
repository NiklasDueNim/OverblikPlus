using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);


var environment = builder.Configuration["ENVIRONMENT"] ?? "dev";
var taskApiBaseUrl = builder.Configuration["TASK_API_BASE_URL"] ?? "https://fallback-task.example.com";
var userApiBaseUrl = builder.Configuration["USER_API_BASE_URL"] ?? "https://fallback-user.example.com";


Console.WriteLine($"Environment: {environment}");
Console.WriteLine($"TASK API Base URL: {taskApiBaseUrl}");
Console.WriteLine($"USER API Base URL: {userApiBaseUrl}");


builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddSingleton<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<JwtAuthorizationMessageHandler>(provider =>
    new JwtAuthorizationMessageHandler(provider.GetRequiredService<CustomAuthStateProvider>())
        .ConfigureHandler(authorizedUrls: new[]
        {
            taskApiBaseUrl,
            userApiBaseUrl
        }));


builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri(userApiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<ICalendarEventService, CalendarEventService>(client =>
{
    client.BaseAddress = new Uri(taskApiBaseUrl);
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


builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAuthorizationCore();


await builder.Build().RunAsync();
