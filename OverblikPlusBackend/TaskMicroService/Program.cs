using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using OverblikPlus.Shared.Interfaces;
using OverblikPlus.Shared.Logging;
using Serilog;
using TaskMicroService.DataAccess;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.dtos.Task;
using TaskMicroService.Services;
using TaskMicroService.Services.Interfaces;
using TaskMicroService.Validators;
using TaskMicroService.Middlewares;
using TaskMicroService.Validators.Calendar;

// ---- ENVIRONMENT LOGGING ----
var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

// ---- SERILOG CONFIGURATION ----
// Log.Logger = new LoggerConfiguration()
//     .ReadFrom.Configuration(builder.Configuration)
//     .Enrich.FromLogContext()
//     .WriteTo.Console()
//     .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
//     .WriteTo.ApplicationInsights(
//         Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING"), 
//         TelemetryConverter.Traces)
//     .CreateLogger();

builder.Host.UseSerilog();

// ---- APPLICATION INSIGHTS ----
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
});

// ---- DATABASE CONFIGURATION ----
// Henter DB fra env var, fallback til en default (localhost)
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
    ?? "Server=localhost,1433;Database=Overblikplus_Dev;User Id=sa;Password=reallyStrongPwd123;";

Log.Logger.Information("This is the DB connection string: " + dbConnectionString);

builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

// ---- JWT CONFIGURATION ----
// Læser env vars til JWT, fallback til appsettings.json
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
var jwtKey     = Environment.GetEnvironmentVariable("JWT_KEY")     ?? builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ---- Blob Storage ----
var blobConnectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STRING") 
    ?? "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://localhost:10000;";
builder.Services.AddSingleton(x => new BlobServiceClient(blobConnectionString));
Log.Logger.Information($"Blob Storage Connection String: {blobConnectionString}");

// Dynamisk generer base URL baseret på miljø
var blobBaseUrl = Environment.GetEnvironmentVariable("BLOB_BASE_URL") 
    ?? "http://localhost:10000/devstoreaccount1/images";
builder.Services.AddSingleton(blobBaseUrl);
Log.Logger.Information($"Blob Base URL: {blobBaseUrl}");

// ---- CORS ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOverblikPlus",
        policy => policy.WithOrigins(
            "https://overblikplus.dk",
            "https://yellow-ocean-0f63e7903.4.azurestaticapps.net",
            "http://localhost:5226")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// ---- SERVICES ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<ILoggerService, LoggerService>();

// ---- DEPENDENCY INJECTION ----
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskStepService, TaskStepService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();

// ---- VALIDATORS ----
builder.Services.AddScoped<IValidator<UpdateTaskDto>, UpdateTaskDtoValidator>();
builder.Services.AddScoped<IValidator<CreateTaskDto>, CreateTaskDtoValidator>();
builder.Services.AddScoped<IValidator<CreateCalendarEventDto>, CreateCalendarEventDtoValidator>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

// ---- BUILD APPLICATION ----
var app = builder.Build();

// ---- MIDDLEWARE CONFIGURATION ----
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowOverblikPlus");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---- START APP ----
try
{
    Log.Information("Starting the application in {Environment} mode", environment);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
