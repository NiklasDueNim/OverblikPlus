using System.Text;
using Azure.Storage.Blobs;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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
        // 1) Konfigurer Serilog til console (simpelt eksempel).
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        // 2) Brug Serilog som logging.
        builder.Host.UseSerilog();

        // 3) Læs environment (Development / Production / etc.)
        var environment = builder.Environment.EnvironmentName;

        // 4) Application Insights - valgfrit
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]
                ?? "InstrumentationKey=b97b1b86-165e-4cfd-a348-149f9d0c992d";
        });
        builder.Logging.AddApplicationInsights(
            config =>
            {
                config.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]
                    ?? "InstrumentationKey=b97b1b86-165e-4cfd-a348-149f9d0c992d";
            },
            _ => { }
        );

        // 5) Registrér vores eget logger-service
        builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
        builder.Services.AddSingleton<ILoggerService, LoggerService>();
        
        // Byg midlertidigt en serviceprovider, så vi kan logge tidligt
        var tempProvider = builder.Services.BuildServiceProvider();
        var logger = tempProvider.GetRequiredService<ILoggerService>();

        // 6) Database: Læs DBConnectionString (placeholder i appsettings.json)
        var dbConnectionString = builder.Configuration.GetConnectionString("DBConnectionString");
        logger.LogInfo($"[TaskMicroService] DB Connection String: {dbConnectionString}");

        // Registrer EF DbContext
        builder.Services.AddDbContext<TaskDbContext>(options =>
            options.UseSqlServer(dbConnectionString));

        // 7) Læs JWT-settings fra config (som er erstattet af sed i GitHub Actions)
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];       // "IssuerPlaceholder" før sed
        var jwtAudience = builder.Configuration["Jwt:Audience"];   // "AudiencePlaceholder" før sed
        var jwtKey = builder.Configuration["Jwt:Key"];            // "KeyPlaceholder" før sed

        // Vil du have flere audiences? 
        // -> parse evt. semikolon-separeret streng i stedet for blot 'jwtAudience'.

        IdentityModelEventSource.ShowPII = true; // Viser flere detaljer i fejl

        logger.LogInfo($"[TaskMicroService] JWT Issuer: {jwtIssuer}");
        logger.LogInfo($"[TaskMicroService] JWT Audience: {jwtAudience}");
        logger.LogInfo($"[TaskMicroService] JWT Key Length: {jwtKey?.Length}");

        // 8) Konfigurer JWT-bearer
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
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
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        // 9) BlobStorage
        var blobConnectionString = builder.Configuration.GetConnectionString("BlobStorageConnectionString");
        logger.LogInfo($"[TaskMicroService] Blob Connection: {blobConnectionString}");
        builder.Services.AddSingleton(_ => new BlobServiceClient(blobConnectionString));

        var blobBaseUrl = builder.Configuration["BLOB_BASE_URL"]; // "BlobStorageBaseUrlPlaceholder" før sed
        builder.Services.AddSingleton(blobBaseUrl);
        logger.LogInfo($"[TaskMicroService] Blob Base Url: {blobBaseUrl}");

        // 10) CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins(
                            // Du kan evt. også sætte disse via placeholders hvis du vil
                            "https://yellow-ocean-0f63e7903.4.azurestaticapps.net",
                            "https://overblikplus.dk"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Authorization");
                });
        });

        // 11) Registrer services / controllers / swagger / AutoMapper
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // 12) DI (TaskMicroService)
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<ITaskStepService, TaskStepService>();
        builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();

        // 13) FluentValidation
        builder.Services.AddScoped<IValidator<UpdateTaskDto>, UpdateTaskDtoValidator>();
        builder.Services.AddScoped<IValidator<CreateTaskDto>, CreateTaskDtoValidator>();
        builder.Services.AddScoped<IValidator<CreateCalendarEventDto>, CreateCalendarEventDtoValidator>();
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

        // 14) Byg applikationen
        var app = builder.Build();

        // 15) Middleware - Statuskoder
        app.UseStatusCodePages(context =>
        {
            var response = context.HttpContext.Response;
            if (response.StatusCode == 301 || response.StatusCode == 302)
            {
                response.StatusCode = 403;
            }
            return Task.CompletedTask;
        });

        // 16) Håndter OPTIONS/CORS
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

        // 17) Forwarded headers (typisk på Azure)
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });

        // 18) Swagger
        app.UseSwagger();
        app.UseSwaggerUI();

        // 19) Dev-exceptions (fjern i prod om ønsket)
        app.UseDeveloperExceptionPage();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowSpecificOrigins");

        // 20) Custom error-handling middleware
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // 21) Auth
        app.UseAuthentication();
        app.UseAuthorization();

        // 22) Map controllers
        app.MapControllers();

        try
        {
            logger.LogInfo($"[TaskMicroService] Starting application in {environment} mode.");
            app.Run();
        }
        catch (Exception ex)
        {
            logger.LogError("Application start-up failed", ex);
        }
    }
}