using System.Security.Claims;
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
using TaskMicroService.Services;
using TaskMicroService.Services.Interfaces;
using TaskMicroService.Validators.Calendar;
using TaskMicroService.Validators.Tasks;
using TaskMicroService.Repositories.Interfaces;
using SeedData;
using TaskMicroService.Middlewares;

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? "")),

                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        logger.LogError($"[JWT] Authentication failed: {context.Exception.Message}", context.Exception);
                        logger.LogError($"[JWT] Exception type: {context.Exception.GetType().Name}", context.Exception);
                        if (context.Exception is SecurityTokenException stEx)
                        {
                            logger.LogError($"[JWT] SecurityTokenException details: {stEx.Message}", stEx);
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        logger.LogWarning($"[JWT] Challenge triggered. Error: {context.Error}, ErrorDescription: {context.ErrorDescription}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var userId = context.Principal?.FindFirst("nameid")?.Value;
                        logger.LogInfo($"[JWT] Token validated successfully for user: {userId}");
                        return Task.CompletedTask;
                    }
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
            builder.Services.AddSingleton(_ => new BlobServiceClient("UseDevelopmentStorage=true"));
            builder.Services.AddSingleton("http://localhost:10000/devstoreaccount1");
        }

        builder.Services.AddCors(options =>
        {
            // Development CORS policy (less restrictive)
            options.AddPolicy("AllowLocalDev",
                policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:5226",  // Rider Frontend
                            "http://localhost:5003"   // Docker Frontend
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetPreflightMaxAge(TimeSpan.FromHours(1));
                });

            // Production CORS policy
            options.AddPolicy("AllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins(
                            "https://nice-wave-08dd97903.1.azurestaticapps.net",  // PROD Static Web App
                            "https://overblikplus.dk",                              // PROD Custom Domain
                            "https://witty-meadow-0c52c9003.2.azurestaticapps.net", // DEV Static Web App
                            "http://localhost:5226",                                 // Local Development (Rider Frontend)
                            "http://localhost:5003"                                  // Docker Development (Docker Frontend)
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Authorization")
                        .SetPreflightMaxAge(TimeSpan.FromHours(1));
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
        // Repositories
        builder.Services.AddScoped<IShiftRepository, Repositories.ShiftRepository>();
        builder.Services.AddScoped<Repositories.Interfaces.ICalendarEventRepository, Repositories.CalendarEventRepository>();
        builder.Services.AddScoped<Repositories.Interfaces.IBudgetRepository, Repositories.BudgetRepository>();
        builder.Services.AddScoped<Repositories.Interfaces.IMoodRepository, Repositories.MoodRepository>();
        builder.Services.AddScoped<Repositories.Interfaces.ITaskStepRepository, Repositories.TaskStepRepository>();
        builder.Services.AddScoped<Repositories.Interfaces.IActivityRepository, Repositories.ActivityRepository>();
        builder.Services.AddScoped<Repositories.Interfaces.IFacilityRepository, Repositories.FacilityRepository>();
        builder.Services.AddScoped<Repositories.Interfaces.ITaskRepository, Repositories.TaskRepository>();
        
        // Services
        builder.Services.AddScoped<Services.Recurrence.IRecurrenceCalculator, Services.Recurrence.RecurrenceCalculator>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<ITaskStepService, TaskStepService>();
        builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
        builder.Services.AddScoped<IActivityService, ActivityService>();
        builder.Services.AddScoped<IFacilityService, FacilityService>();
        builder.Services.AddScoped<IShiftService, ShiftService>();
        builder.Services.AddScoped<ITaskDbContext, TaskDbContext>();
        builder.Services.AddScoped<IImageService, ImageService>();
        builder.Services.AddScoped<IRelativeService, RelativeService>();
        builder.Services.AddScoped<IBudgetService, BudgetService>();
        
        // Configure HttpClient for UserMicroService API
        var userApiBaseUrl = builder.Configuration["UserApiBaseUrl"] ?? "http://localhost:5004";
        builder.Services.AddHttpClient("UserApi", client =>
        {
            client.BaseAddress = new Uri(userApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddScoped<IMoodService, MoodService>();


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

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHttpsRedirection();
        }
        
        app.UseRouting();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowLocalDev");
        }
        else
        {
            app.UseCors("AllowSpecificOrigins");
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskMicroService API V1");
            c.RoutePrefix = "swagger";
        });

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers()
            .RequireCors(app.Environment.IsDevelopment() ? "AllowLocalDev" : "AllowSpecificOrigins");
        
        var seeder = new DatabaseSeeder<TaskDbContext>(logger, builder.Environment);
        await seeder.SeedAsync(app.Services);
        
        logger.LogInfo($"[TaskMicroService] Starting application in {environment} mode.");
        await app.RunAsync();
    }
}
