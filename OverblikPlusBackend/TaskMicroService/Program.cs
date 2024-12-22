using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using TaskMicroService.DataAccess;
using TaskMicroService.dto;
using TaskMicroService.Profiles;
using TaskMicroService.Services;
using TaskMicroService.Services.Interfaces;
using TaskMicroService.Validators;
using TaskMicroService.Middelwares;
using DotNetEnv;

// ---- ENVIRONMENT LOGGING ----
DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

// ---- SERILOG CONFIGURATION ----
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.ApplicationInsights(
        Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING"), 
        TelemetryConverter.Traces)
    .CreateLogger();

builder.Host.UseSerilog();

// ---- APPLICATION INSIGHTS ----
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
});

// ---- DATABASE CONFIGURATION ----
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

// ---- JWT AUTHENTICATION ----
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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
var blobConnectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STRING");
builder.Services.AddSingleton(x =>
{
    return new BlobServiceClient(blobConnectionString);
});

// ---- CORS CONFIGURATION ----
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll",
            policy => policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    }
    else
    {
        options.AddPolicy("AllowProduction",
            policy => policy.WithOrigins("https://overblikplus.dk")
                .AllowAnyMethod()
                .AllowAnyHeader());
    }
});

// ---- SERVICES ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ---- DEPENDENCY INJECTION ----
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskStepService, TaskStepService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// ---- VALIDATORS ----
builder.Services.AddScoped<IValidator<UpdateTaskDto>, UpdateTaskDtoValidator>();
builder.Services.AddScoped<IValidator<CreateTaskDto>, CreateTaskDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

// ---- BUILD APPLICATION ----
var app = builder.Build();

// ---- MIDDLEWARE CONFIGURATION ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowProduction");
}

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
