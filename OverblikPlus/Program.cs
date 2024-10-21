using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;
using OverblikPlus;
using OverblikPlus.Services;
using AutoMapper;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Tilføj HttpClient for at kommunikere med API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5081/") });

// Registrer dine services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();

// AutoMapper konfiguration
builder.Services.AddAutoMapper(typeof(MappingProfile));


await builder.Build().RunAsync();