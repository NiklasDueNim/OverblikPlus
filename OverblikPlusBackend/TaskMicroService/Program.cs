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

        var dbConnectionString = builder.Configuration.GetConnectionString("DBConnectionString");
        logger.LogInfo($"[TaskMicroService] DB Connection String: {dbConnectionString}");


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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        var blobConnectionString = builder.Configuration.GetConnectionString("BlobStorageConnectionString");
        logger.LogInfo($"[TaskMicroService] Blob Connection: {blobConnectionString}");
        builder.Services.AddSingleton(_ => new BlobServiceClient(blobConnectionString));

        var blobBaseUrl = builder.Configuration["BLOB_BASE_URL"];
        builder.Services.AddSingleton(blobBaseUrl);
        logger.LogInfo($"[TaskMicroService] Blob Base Url: {blobBaseUrl}");

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
}// Backend workflow test trigger
