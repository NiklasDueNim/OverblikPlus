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


        var dbConnectionString = builder.Configuration.GetConnectionString("DBConnectionString");
        Console.WriteLine($"DB_CONNECTION_STRING: {dbConnectionString}");
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new Exception("DB_CONNECTION_STRING is missing or empty.");
        }

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(dbConnectionString, x => x.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));
      
        var encryptionKeyBase64 = builder.Configuration["EncryptionSettings:EncryptionKey"];
        if (string.IsNullOrEmpty(encryptionKeyBase64))
        {
            throw new InvalidOperationException("Encryption key is missing.");
        }
        
        // Decode Base64 key to get the raw 32-byte key
        string encryptionKey;
        try
        {
            var keyBytes = Convert.FromBase64String(encryptionKeyBase64);
            encryptionKey = Encoding.UTF8.GetString(keyBytes);
            
            // Ensure key is exactly 32 characters for AES-256
            if (encryptionKey.Length > 32)
            {
                encryptionKey = encryptionKey.Substring(0, 32);
            }
            else if (encryptionKey.Length < 32)
            {
                encryptionKey = encryptionKey.PadRight(32, '0');
            }
        }
        catch (FormatException)
        {
            // If not Base64, use as-is and ensure 32 characters
            encryptionKey = encryptionKeyBase64.Length > 32 ? encryptionKeyBase64.Substring(0, 32) : encryptionKeyBase64.PadRight(32, '0');
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