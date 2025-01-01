using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using OverblikPlus.Shared.Interfaces;
using OverblikPlus.Shared.Logging;
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

// ---- LOGGER SERVICE ----
builder.Services.AddSingleton<ILoggerService, LoggerService>();
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerService>();

// ---- DATABASE CONFIGURATION ----
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
    ?? "Server=localhost,1433;Database=Overblikplus_Dev;User Id=sa;Password=reallyStrongPwd123;";
logger.LogInfo($"Database Connection String: {dbConnectionString}");

builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

// ---- JWT CONFIGURATION ----
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

logger.LogInfo($"JWT Issuer: {jwtIssuer}");
logger.LogInfo($"JWT Audience: {jwtAudience}");

// ---- Blob Storage ----
var blobConnectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STRING") 
    ?? "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://localhost:10000;";
builder.Services.AddSingleton(x => new BlobServiceClient(blobConnectionString));
logger.LogInfo($"Blob Storage Connection String: {blobConnectionString}");

var blobBaseUrl = Environment.GetEnvironmentVariable("BLOB_BASE_URL") 
    ?? "http://localhost:10000/devstoreaccount1/images";
builder.Services.AddSingleton(blobBaseUrl);
logger.LogInfo($"Blob Base URL: {blobBaseUrl}");

// ---- CORS ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ---- SERVICES ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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

// Global middleware til logging
app.Use(async (context, next) =>
{
    try
    {
        logger.LogInfo($"Request: {context.Request.Method} {context.Request.Path}");
        await next.Invoke();
        logger.LogInfo($"Response: {context.Response.StatusCode}");
    }
    catch (Exception ex)
    {
        logger.LogError("Unhandled exception caught in middleware.", ex);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    }
});

app.UseCors("AllowAll");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    logger.LogInfo("Starting the application in {Environment} mode".Replace("{Environment}", environment));
    app.Run();
}
catch (Exception ex)
{
    logger.LogError("Application start-up failed", ex);
}
