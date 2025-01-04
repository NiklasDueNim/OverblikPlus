using System.Text;
using Azure.Storage.Blobs;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using OverblikPlus.Shared.Interfaces;
using OverblikPlus.Shared.Logging;
using Serilog;
using TaskMicroService.DataAccess;
using TaskMicroService.Dtos.Calendar;
using TaskMicroService.dtos.Task;
using TaskMicroService.Entities;
using TaskMicroService.Middlewares;
using TaskMicroService.Services;
using TaskMicroService.Services.Interfaces;
using TaskMicroService.Validators;
using TaskMicroService.Validators.Calendar;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        // Register Serilog
        builder.Host.UseSerilog();

        var environment = builder.Environment.EnvironmentName;

        // ---- Application Insights ----
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = "InstrumentationKey=b97b1b86-165e-4cfd-a348-149f9d0c992d";
        });
        builder.Logging.AddApplicationInsights(
            configureTelemetryConfiguration: (config) =>
                config.ConnectionString = "InstrumentationKey=b97b1b86-165e-4cfd-a348-149f9d0c992d",
            configureApplicationInsightsLoggerOptions: (options) => { });

        // ---- LOGGER SERVICE ----
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddSingleton<ILoggerService, LoggerService>();
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerService>();

        // ---- DATABASE CONFIGURATION ----
        var dbConnectionString = builder.Configuration["ConnectionStrings:DBConnectionString"];
        logger.LogInfo($"Database Connection String: {dbConnectionString}");

        builder.Services.AddDbContext<TaskDbContext>(options =>
            options.UseSqlServer(dbConnectionString));

        // ---- JWT CONFIGURATION ----
        var jwtIssuer = "https://overblikplus-user-api-dev-cheeh0a0fgc0ayh5.westeurope-01.azurewebsites.net";
        var jwtAudience = "https://overblikplus-task-api-dev-aqcja5a8htcwb8fp.westeurope-01.azurewebsites.net";
        var jwtKey = "MyVeryStrongSecretKeyForJWT1234567890123456789";

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtIssuer,
                    ValidAudiences = new[]
                    {
                        "https://yellow-ocean-0f63e7903.4.azurestaticapps.net",
                        "https://overblikplus-task-api-dev-aqcja5a8htcwb8fp.westeurope-01.azurewebsites.net"
                    },
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        IdentityModelEventSource.ShowPII = true;

        logger.LogInfo($"JWT Issuer in Runtime: {jwtIssuer}");
        logger.LogInfo($"JWT Audience in Runtime: {jwtAudience}");
        logger.LogInfo($"JWT Key Length: {jwtKey.Length}");

        // ---- Blob Storage ----
        var blobConnectionString = builder.Configuration["ConnectionStrings:BlobStorageConnectionString"];

        builder.Services.AddSingleton(x => new BlobServiceClient(blobConnectionString));
        logger.LogInfo($"Blob Storage Connection String: {blobConnectionString}");

        var blobBaseUrl = builder.Configuration["BLOB_BASE_URL"];

        builder.Services.AddSingleton(blobBaseUrl);
        logger.LogInfo($"Blob Base URL: {blobBaseUrl}");

        // ---- CORS ----
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins(
                            "https://yellow-ocean-0f63e7903.4.azurestaticapps.net", // Dev
                            "https://overblikplus.dk" // Prod
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Authorization");
                });
        });


        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TaskDbContext>()
            .AddDefaultTokenProviders();

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

        app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;
            if (response.StatusCode == 301 || response.StatusCode == 302)
            {
                response.StatusCode = 403;
            }
        });

        app.Use(async (context, next) =>
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Content-Type");
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                await context.Response.CompleteAsync();
                return;
            }

            await next();
        });

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseDeveloperExceptionPage();
        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors("AllowSpecificOrigins");
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
    }
}
