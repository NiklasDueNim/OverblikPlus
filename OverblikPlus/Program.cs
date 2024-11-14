using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OverblikPlus;
using OverblikPlus.Services;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Tilføj CustomAuthStateProvider som service til brug af JWT
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<AuthService>();

// Konfigurer JWT-håndtering i HTTP-klienterne
builder.Services.AddScoped<JwtAuthorizationMessageHandler>(provider => new JwtAuthorizationMessageHandler(provider.GetRequiredService<CustomAuthStateProvider>())
    .ConfigureHandler(authorizedUrls: new[] { "https://overblikplus-task-api.azurewebsites.net", "https://overblikplus-user-api.azurewebsites.net" }));

// Tilføj HttpClient services med JWT autorisation
builder.Services.AddHttpClient<ITaskService, TaskService>(client =>
{
    client.BaseAddress = new Uri("https://overblikplus-task-api.azurewebsites.net");
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri("https://overblikplus-user-api.azurewebsites.net");
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<ITaskStepService, TaskStepService>(client =>
{
    client.BaseAddress = new Uri("https://overblikplus-task-api.azurewebsites.net");
}).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// Tilføjelse af AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Konfiguration af autorisation
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();