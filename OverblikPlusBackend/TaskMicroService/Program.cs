using System.Text;
using Azure.Storage.Blobs;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
using TaskMicroService.Validators.Tasks;

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

        var jwtIssuer = builder.Configuration["Jwt:Issuer"];       
        var jwtAudience = builder.Configuration["Jwt:Audience"];   
        var jwtKey = builder.Configuration["Jwt:Key"];            

        

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

        var blobConnectionString = builder.Configuration.GetConnectionString("BlobStorageConnectionString");
        var blobBaseUrl = builder.Configuration["BlobStorageBaseUrl"];
        
        if (!string.IsNullOrEmpty(blobConnectionString) && !string.IsNullOrEmpty(blobBaseUrl))
        {
            builder.Services.AddSingleton(_ => new BlobServiceClient(blobConnectionString));
            builder.Services.AddSingleton(blobBaseUrl);
        }
        else
        {
            // Fallback for local development
            builder.Services.AddSingleton(_ => new BlobServiceClient("UseDevelopmentStorage=true"));
            builder.Services.AddSingleton("http://localhost:10000/devstoreaccount1");
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins(
                            "https://nice-wave-08dd97903.1.azurestaticapps.net",  // PROD Static Web App
                            "https://overblikplus.dk",                              // PROD Custom Domain
                            "https://witty-meadow-0c52c9003.2.azurestaticapps.net", // DEV Static Web App
                            "http://localhost:5226"                                 // Local Development
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Authorization");
                });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskMicroService API", Version = "v1" });
            
            // Add JWT Bearer Authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });
        builder.Services.AddControllers();
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<ITaskStepService, TaskStepService>();
        builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
        builder.Services.AddScoped<IActivityService, ActivityService>();
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

        // Fjernes i produktion
        app.UseDeveloperExceptionPage();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowSpecificOrigins");

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskMicroService API V1");
            c.RoutePrefix = "swagger"; // Standard Swagger path
        });

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
