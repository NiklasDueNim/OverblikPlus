using System.Text;
using Azure.Storage.Blobs;
using FluentValidation;
using FluentValidation.AspNetCore;
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
using TaskMicroService.Middlewares;
using TaskMicroService.Services;
using TaskMicroService.Services.Interfaces;
using TaskMicroService.Validators;
using TaskMicroService.Validators.Calendar;

namespace TaskMicroService;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        var environment = builder.Environment.EnvironmentName;

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

        
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddSingleton<ILoggerService, LoggerService>();
        
       
        var tempProvider = builder.Services.BuildServiceProvider();
        var logger = tempProvider.GetRequiredService<ILoggerService>();

        // === EXTENSIVE LOGGING FOR DEBUGGING ===
        Console.WriteLine("=== TASK MICROSERVICE STARTUP DEBUGGING ===");
        Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
        Console.WriteLine($"ContentRoot: {builder.Environment.ContentRootPath}");
        Console.WriteLine($"Args: [{string.Join(", ", args)}]");
        
        // Log all configuration sources
        Console.WriteLine("\n=== CONFIGURATION SOURCES ===");
        foreach (var source in builder.Configuration.Sources)
        {
            Console.WriteLine($"Config Source: {source.GetType().Name}");
        }
        
        // Log all configuration keys and values (be careful with sensitive data)
        Console.WriteLine("\n=== ALL CONFIGURATION KEYS ===");
        foreach (var kvp in builder.Configuration.AsEnumerable())
        {
            var value = kvp.Value;
            // Mask sensitive values
            if (kvp.Key.Contains("Key", StringComparison.OrdinalIgnoreCase) || 
                kvp.Key.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains("Secret", StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains("Connection", StringComparison.OrdinalIgnoreCase))
            {
                value = string.IsNullOrEmpty(value) ? "[NULL/EMPTY]" : $"[MASKED-{value.Length}chars]";
            }
            Console.WriteLine($"  {kvp.Key} = {value}");
        }
        
        Console.WriteLine("\n=== SPECIFIC CONFIG LOOKUPS ===");

        var dbConnectionString = builder.Configuration.GetConnectionString("DBConnectionString");
        logger.LogInfo($"[TaskMicroService] DB Connection String: {dbConnectionString ?? "NULL"}");
        
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            logger.LogError("[TaskMicroService] DB Connection String is missing or empty. Available connection strings:", new InvalidOperationException("DB Connection String is missing"));
            foreach (var connStr in builder.Configuration.GetSection("ConnectionStrings").GetChildren())
            {
                logger.LogError($"  - {connStr.Key}: {connStr.Value}", new InvalidOperationException("Configuration issue"));
            }
            // Don't throw exception, just log and continue
            logger.LogError("[TaskMicroService] Continuing without database connection - this will likely cause issues later.", new InvalidOperationException("DB Connection missing"));
        }


        builder.Services.AddDbContext<TaskDbContext>(options =>
            options.UseSqlServer(dbConnectionString, x => x.MigrationsAssembly(typeof(TaskDbContext).Assembly.FullName)));

        // JWT Configuration with detailed logging
        Console.WriteLine("\n=== JWT CONFIGURATION (TASK API) ===");
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? builder.Configuration["Jwt__Issuer"];       
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? builder.Configuration["Jwt__Audience"];   
        var jwtKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt__Key"];

        Console.WriteLine($"Jwt:Issuer = {jwtIssuer ?? "[NULL/EMPTY]"}");
        Console.WriteLine($"Jwt:Audience = {jwtAudience ?? "[NULL/EMPTY]"}");
        Console.WriteLine($"Jwt:Key = {(string.IsNullOrEmpty(jwtKey) ? "[NULL/EMPTY]" : $"[FOUND-{jwtKey.Length}chars]")}");
        
        // Also try the double underscore versions
        Console.WriteLine($"Jwt__Issuer = {builder.Configuration["Jwt__Issuer"] ?? "[NULL/EMPTY]"}");
        Console.WriteLine($"Jwt__Audience = {builder.Configuration["Jwt__Audience"] ?? "[NULL/EMPTY]"}");
        Console.WriteLine($"Jwt__Key = {(string.IsNullOrEmpty(builder.Configuration["Jwt__Key"]) ? "[NULL/EMPTY]" : $"[FOUND-{builder.Configuration["Jwt__Key"].Length}chars]")}");

        if (string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) || string.IsNullOrEmpty(jwtKey))
        {
            Console.WriteLine("WARNING: JWT configuration is missing or incomplete!");
            logger.LogError("[TaskMicroService] JWT configuration is missing or incomplete.", new InvalidOperationException("JWT configuration missing"));
            logger.LogError($"[TaskMicroService] JWT Issuer: {jwtIssuer ?? "NULL"}", new InvalidOperationException("JWT Issuer missing"));
            logger.LogError($"[TaskMicroService] JWT Audience: {jwtAudience ?? "NULL"}", new InvalidOperationException("JWT Audience missing"));
            logger.LogError($"[TaskMicroService] JWT Key: {(jwtKey != null ? "SET" : "NULL")}", new InvalidOperationException("JWT Key missing"));
            // Don't throw exception, just log and continue
            logger.LogError("[TaskMicroService] Continuing without JWT configuration - this will likely cause issues later.", new InvalidOperationException("JWT configuration incomplete"));
        }
        else
        {
            Console.WriteLine("JWT configuration appears complete!");
        }            

        

        IdentityModelEventSource.ShowPII = true;

        logger.LogInfo($"[TaskMicroService] JWT Issuer: {jwtIssuer}");
        logger.LogInfo($"[TaskMicroService] JWT Audience: {jwtAudience}");
        logger.LogInfo($"[TaskMicroService] JWT Key Length: {jwtKey?.Length}");

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""))
                };
            });

        // Blob Storage Configuration with detailed logging
        Console.WriteLine("\n=== BLOB STORAGE CONFIGURATION ===");
        var blobConnectionString = builder.Configuration.GetConnectionString("BlobStorageConnectionString");
        Console.WriteLine($"BlobStorageConnectionString = {(string.IsNullOrEmpty(blobConnectionString) ? "[NULL/EMPTY]" : $"[FOUND-{blobConnectionString.Length}chars]")}");
        logger.LogInfo($"[TaskMicroService] Blob Connection: {blobConnectionString ?? "NULL"}");
        
        if (string.IsNullOrEmpty(blobConnectionString))
        {
            Console.WriteLine("WARNING: Blob Storage Connection String is missing or empty.");
            logger.LogError("[TaskMicroService] Blob Storage Connection String is missing or empty.", new InvalidOperationException("Blob connection string missing"));
            // Don't throw exception, just log and continue
            logger.LogError("[TaskMicroService] Continuing without blob storage - this will likely cause issues later.", new InvalidOperationException("Blob storage unavailable"));
        }
        else
        {
            Console.WriteLine("Blob Storage Connection String found, creating BlobServiceClient...");
            logger.LogInfo($"[TaskMicroService] Blob Connection String: {blobConnectionString.Substring(0, Math.Min(50, blobConnectionString.Length))}...");
            builder.Services.AddSingleton(_ => new BlobServiceClient(blobConnectionString));
            Console.WriteLine("BlobServiceClient successfully created.");
        }

        var blobBaseUrl = builder.Configuration["BLOB_BASE_URL"];
        Console.WriteLine($"BLOB_BASE_URL = {blobBaseUrl ?? "[NULL/EMPTY]"}");
        if (!string.IsNullOrEmpty(blobBaseUrl))
        {
            builder.Services.AddSingleton(blobBaseUrl);
            Console.WriteLine("Blob Base URL successfully registered as singleton.");
        }
        logger.LogInfo($"[TaskMicroService] Blob Base Url: {blobBaseUrl ?? "NULL"}");

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins(
                            "https://yellow-ocean-0f63e7903.4.azurestaticapps.net",
                            "https://overblikplus.dk",
                            "http://localhost:5226"
                            
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Authorization");
                });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<ITaskStepService, TaskStepService>();
        builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
        builder.Services.AddScoped<ITaskDbContext, TaskDbContext>();
        builder.Services.AddScoped<IImageService, ImageService>();
        builder.Services.AddScoped<IRelativeService, RelativeService>();


        builder.Services.AddScoped<IValidator<UpdateTaskDto>, UpdateTaskDtoValidator>();
        builder.Services.AddScoped<IValidator<CreateTaskDto>, CreateTaskDtoValidator>();
        builder.Services.AddScoped<IValidator<CreateCalendarEventDto>, CreateCalendarEventDtoValidator>();
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

        var app = builder.Build();

        app.UseStatusCodePages(context =>
        {
            var response = context.HttpContext.Response;
            if (response.StatusCode == 301 || response.StatusCode == 302)
            {
                response.StatusCode = 403;
            }
            return Task.CompletedTask;
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

        // Fjernes i produktion
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
            // Auto-migrate database in Development and Production mode
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
                
                // Log connection string for debugging
                var connectionString = context.Database.GetConnectionString();
                logger.LogInfo($"[TaskMicroService] Database connection string: {connectionString}");
                
                // Also log environment variables
                var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
                logger.LogInfo($"[TaskMicroService] Environment ConnectionStrings__DefaultConnection: {envConnectionString}");
                
                try
                {
                    await context.Database.MigrateAsync();
                    logger.LogInfo("[TaskMicroService] Database migrations completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Migration failed: {ex.Message}", ex);
                    // Try to ensure database is created if migration fails
                    try
                    {
                        await context.Database.EnsureCreatedAsync();
                        logger.LogInfo("[TaskMicroService] Database ensured created.");
                    }
                    catch (Exception ensureEx)
                    {
                        logger.LogError($"EnsureCreated failed: {ensureEx.Message}", ensureEx);
                        // Continue anyway - app should still start
                        logger.LogInfo("[TaskMicroService] Continuing despite database setup failure.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Database setup failed", ex);
        }
        
        logger.LogInfo($"[TaskMicroService] Starting application in {environment} mode.");
        await app.RunAsync();
    }
}

