using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

namespace UserMicroService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Serilog: Skriver til Console (kan udvides) ---
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration) // læser evt. serilog-indstillinger fra config
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();

        // --- Registrer Serilog.ILogger som singleton ---
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddSingleton<ILoggerService, LoggerService>();

        // Byg en midlertidig serviceprovider for at kunne logge tidligt
        var tempProvider = builder.Services.BuildServiceProvider();
        var logger = tempProvider.GetRequiredService<ILoggerService>();

        // --- DATABASE CONNECTION ---
        // Læser placeholder "DBConnectionStringPlaceholder" fra appsettings.json (overskrives i GitHub Actions)
        var dbConnectionString = builder.Configuration.GetConnectionString("DBConnectionString");
        Console.WriteLine($"DB_CONNECTION_STRING: {dbConnectionString}");
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new Exception("DB_CONNECTION_STRING is missing or empty.");
        }

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(dbConnectionString));

        // --- ENCRYPTION KEY ---
        // appsettings.json -> "EncryptionSettings": { "EncryptionKey": "EncryptionKeyPlaceholder" }
        var encryptionKey = builder.Configuration["EncryptionSettings:EncryptionKey"];
        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new InvalidOperationException("Encryption key is missing.");
        }
        EncryptionHelper.SetEncryptionKey(encryptionKey);

        // --- IDENTITY CONFIGURATION ---
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

        // --- JWT CONFIGURATION ---
        // Læs placeholders "IssuerPlaceholder", "AudiencePlaceholder", "KeyPlaceholder" fra appsettings.json
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];
        var jwtKey = builder.Configuration["Jwt:Key"];

        // Log info om de læste værdier
        logger.LogInfo($"[UserMicroService] Jwt:Issuer   = {jwtIssuer}");
        logger.LogInfo($"[UserMicroService] Jwt:Audience = {jwtAudience}");
        logger.LogInfo($"[UserMicroService] Jwt:Key Len  = {jwtKey?.Length}");

        // Konfigurer JWT-bearer til at validere indkommende tokens
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

        // --- CORS CONFIGURATION (eksempel) ---
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

        // --- SERVICES & DEPENDENCIES ---
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddAutoMapper(typeof(Program));

        // Register dine custom services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // FluentValidation – fx
        builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
        builder.Services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
        builder.Services.AddScoped<IValidator<ReadUserDto>, ReadUserDtoValidator>();
        builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();

        // --- BUILD APPLICATION ---
        var app = builder.Build();

        // --- MIDDLEWARE & CONFIG ---
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();  // for nem fejlfinding
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            // Du kan stadig vise Swagger i production, hvis du vil
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Log hver request
        app.UseSerilogRequestLogging();

        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        // Map controllers
        app.MapControllers();

        // Kør appen
        try
        {
            logger.LogInfo($"[UserMicroService] Starting in {app.Environment.EnvironmentName} mode.");
            app.Run();
        }
        catch (Exception ex)
        {
            logger.LogError("Application start-up failed", ex);
        }
    }
}
