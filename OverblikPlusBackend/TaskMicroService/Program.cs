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

// ---- ENVIRONMENT LOGGING ----
var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

// ---- SERILOG CONFIGURATION ----
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Henter settings fra appsettings.json
    .Enrich.FromLogContext()
    .WriteTo.Console() // Log til konsol
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day) // Log til fil
    .WriteTo.ApplicationInsights(
        builder.Configuration["ApplicationInsights:ConnectionString"], 
        TelemetryConverter.Traces) // Log til Application Insights
    .CreateLogger();

// Brug Serilog
builder.Host.UseSerilog();

// ---- APPLICATION INSIGHTS ----
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// ---- DATABASE CONFIGURATION ----
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- JWT AUTHENTICATION ----
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});


builder.Services.AddSingleton(x =>
{
    var connectionString = builder.Configuration.GetConnectionString("BlobStorage");
    return new BlobServiceClient(connectionString);
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

// Swagger i Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS Redirect
app.UseHttpsRedirection();

// CORS afhængigt af miljø
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowProduction");
}

// Exception Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Authentication og Authorization
app.UseAuthentication();
app.UseAuthorization();

// Routing
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
    Log.CloseAndFlush(); // Sørg for at flush logs
}
