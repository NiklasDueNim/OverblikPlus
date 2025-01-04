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
        
        // --- LOGGING / SERILOG ---
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        // Register Serilog.ILogger
        builder.Services.AddSingleton(Log.Logger);
        
        // --- DATABASE CONNECTION ---
        var dbConnectionString = builder.Configuration["ConnectionStrings:DBConnectionString"];
        
        Console.WriteLine($"DB_CONNECTION_STRING: {dbConnectionString}");

        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new Exception("DB_CONNECTION_STRING is missing or empty.");
        }

        // --- ENCRYPTION KEY ---
        var encryptionKey = builder.Configuration["EncryptionSettings:EncryptionKey"];

        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new InvalidOperationException("Encryption key is missing.");
        }
        EncryptionHelper.SetEncryptionKey(encryptionKey);
        
        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(dbConnectionString));

        // --- IDENTITY CONFIGURATION ---
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

        // --- JWT CONFIGURATION ---
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = "https://overblikplus-task-api-dev-aqcja5a8htcwb8fp.westeurope-01.azurewebsites.net";

        var jwtKey = builder.Configuration["Jwt:Key"];
                     
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

        Console.WriteLine($"Jwt:Issuer   = {jwtIssuer}");
        Console.WriteLine($"Jwt:Audience = {jwtAudience}");
        Console.WriteLine($"Jwt:Key      = {jwtKey}");

        // --- CORS CONFIGURATION ---
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy => policy.WithOrigins("https://yellow-ocean-0f63e7903.4.azurestaticapps.net",
                        "http://localhost:5226", "https://overblikplus.dk")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        // --- SERVICES & DEPENDENCIES ---
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddAutoMapper(typeof(Program));

        // Registering services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
        builder.Services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
        builder.Services.AddScoped<IValidator<ReadUserDto>, ReadUserDtoValidator>();
        builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        builder.Services.AddSingleton<ILoggerService, LoggerService>();

        // --- BUILD APPLICATION ---
        var app = builder.Build();

        // --- MIDDLEWARE & CONFIG ---
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

        app.MapControllers();
        app.Run();
    }
}