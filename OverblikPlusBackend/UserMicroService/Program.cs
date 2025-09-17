using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OverblikPlus.Shared.Interfaces;
using OverblikPlus.Shared.Logging;
using Serilog;
using UserMicroService.DataAccess;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Helpers;
using UserMicroService.Services;
using UserMicroService.Services.Interfaces;
using UserMicroService.Validators;
using UserMicroService.Validators.Auth;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserMicroService.Hubs;

namespace UserMicroService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddSingleton<ILoggerService, LoggerService>();

        var tempProvider = builder.Services.BuildServiceProvider();
        var logger = tempProvider.GetRequiredService<ILoggerService>();

        // === EXTENSIVE LOGGING FOR DEBUGGING ===
        Console.WriteLine("=== USER MICROSERVICE STARTUP DEBUGGING ===");
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
                kvp.Key.Contains("Secret", StringComparison.OrdinalIgnoreCase))
            {
                value = string.IsNullOrEmpty(value) ? "[NULL/EMPTY]" : $"[MASKED-{value.Length}chars]";
            }
            Console.WriteLine($"  {kvp.Key} = {value}");
        }
        
        // Log environment variables
        Console.WriteLine("\n=== ENVIRONMENT VARIABLES (relevant) ===");
        var envVars = new[] { "ASPNETCORE_ENVIRONMENT", "Encryption__Key", "Jwt__Key", "Jwt__Issuer", "Jwt__Audience", "ConnectionStrings__DBConnectionString" };
        foreach (var envVar in envVars)
        {
            var value = Environment.GetEnvironmentVariable(envVar);
            if (envVar.Contains("Key", StringComparison.OrdinalIgnoreCase))
            {
                value = string.IsNullOrEmpty(value) ? "[NULL/EMPTY]" : $"[MASKED-{value.Length}chars]";
            }
            Console.WriteLine($"  {envVar} = {value}");
        }
        
        Console.WriteLine("\n=== SPECIFIC CONFIG LOOKUPS ===");

        var dbConnectionString = builder.Configuration.GetConnectionString("DBConnectionString");
        Console.WriteLine($"DB_CONNECTION_STRING: {dbConnectionString}");
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new Exception("DB_CONNECTION_STRING is missing or empty.");
        }

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(dbConnectionString, x => x.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));
      
        // Try multiple encryption key locations with detailed logging
        Console.WriteLine("\n=== ENCRYPTION KEY LOOKUP ===");
        var encryptionKey1 = builder.Configuration["EncryptionSettings:EncryptionKey"];
        var encryptionKey2 = builder.Configuration["Encryption:Key"];
        var encryptionKey3 = builder.Configuration["Encryption__Key"];
        var encryptionKey4 = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
        
        Console.WriteLine($"EncryptionSettings:EncryptionKey = {(string.IsNullOrEmpty(encryptionKey1) ? "[NULL/EMPTY]" : $"[FOUND-{encryptionKey1.Length}chars]")}");
        Console.WriteLine($"Encryption:Key = {(string.IsNullOrEmpty(encryptionKey2) ? "[NULL/EMPTY]" : $"[FOUND-{encryptionKey2.Length}chars]")}");
        Console.WriteLine($"Encryption__Key = {(string.IsNullOrEmpty(encryptionKey3) ? "[NULL/EMPTY]" : $"[FOUND-{encryptionKey3.Length}chars]")}");
        Console.WriteLine($"ENV ENCRYPTION_KEY = {(string.IsNullOrEmpty(encryptionKey4) ? "[NULL/EMPTY]" : $"[FOUND-{encryptionKey4.Length}chars]")}");
        
        var encryptionKey = encryptionKey1 ?? encryptionKey2 ?? encryptionKey3 ?? encryptionKey4;
        Console.WriteLine($"Final encryption key selected: {(string.IsNullOrEmpty(encryptionKey) ? "[NULL/EMPTY]" : $"[FOUND-{encryptionKey.Length}chars]")}");
        
        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            Console.WriteLine("ERROR: Encryption key is missing from all sources!");
            throw new InvalidOperationException("Encryption key is missing.");
        }
        EncryptionHelper.SetEncryptionKey(encryptionKey);
        Console.WriteLine("Encryption key successfully set!");

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

        // JWT Configuration with detailed logging
        Console.WriteLine("\n=== JWT CONFIGURATION LOOKUP ===");
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

        logger.LogInfo($"[UserMicroService] Jwt:Issuer   = {jwtIssuer}");
        logger.LogInfo($"[UserMicroService] Jwt:Audience = {jwtAudience}");
        logger.LogInfo($"[UserMicroService] Jwt:Key Len  = {jwtKey?.Length}");

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
        
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("IsSameBosted", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    var userBostedId = context.User.FindFirst("bostedId")?.Value;
                    var requiredBostedId = context.Resource?.ToString();
                    return userBostedId == requiredBostedId;
                });
            });
        });
        
        

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy => policy.WithOrigins(
                        "https://yellow-ocean-0f63e7903.4.azurestaticapps.net",
                        "http://localhost:5226",
                        "https://overblikplus.dk"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddAutoMapper(typeof(Program));

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
        builder.Services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
        builder.Services.AddScoped<IValidator<ReadUserDto>, ReadUserDtoValidator>();
        builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        builder.Services.AddSignalR();
        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(); 
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseSerilogRequestLogging();

        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHub <ChatHub> ("/chatHub");

        app.MapControllers();

        // Auto-migrate database in Development and Production mode
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                var conn = context.Database.GetDbConnection();
                logger.LogInfo($"DB target: {conn.DataSource}/{conn.Database}");
                await context.Database.MigrateAsync();
                logger.LogInfo("[UserMicroService] Database migrations completed successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"DB migration failed at startup - continuing without migration: {ex.Message}", ex);
            // Don't throw - let the app start so we can hit /health and see logs
        }
        
        logger.LogInfo($"[UserMicroService] Starting in {app.Environment.EnvironmentName} mode.");
        await app.RunAsync();
    }
}