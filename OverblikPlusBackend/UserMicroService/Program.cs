using System;
using System.Linq;
using System.Text;
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

        var dbConnectionString = builder.Configuration.GetConnectionString("DBConnectionString");
        Console.WriteLine($"DB_CONNECTION_STRING: {dbConnectionString}");
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new Exception("DB_CONNECTION_STRING is missing or empty.");
        }

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(dbConnectionString));

        var encryptionKey = builder.Configuration["EncryptionSettings:EncryptionKey"];
        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new InvalidOperationException("Encryption key is missing.");
        }
        EncryptionHelper.SetEncryptionKey(encryptionKey);

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];
        var jwtKey = builder.Configuration["Jwt:Key"];

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

        try
        {
            // Auto-migrate database in Development and Production mode
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                await context.Database.MigrateAsync();
                logger.LogInfo("[UserMicroService] Database migrations completed successfully.");
            }
            
            logger.LogInfo($"[UserMicroService] Starting in {app.Environment.EnvironmentName} mode.");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Application start-up failed", ex);
        }
    }
}
// Test backend workflow
// Trigger backend workflow test
// Trigger backend deployment test
// Trigger backend redeployment with database config
// Trigger redeployment with storage settings
// Trigger Azure deployment with build settings
// Fix deployment - disable remote build
// Test Docker workflow deployment
// Trigger workflow manually
