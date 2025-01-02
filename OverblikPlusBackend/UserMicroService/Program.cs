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
        var environment = builder.Environment.EnvironmentName;

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

        // --- ENCRYPTION KEY ---
        var encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY")
                            ?? builder.Configuration["EncryptionSettings:EncryptionKey"];

        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new InvalidOperationException("Encryption key is missing (ENV var or appsettings).");
        }
        EncryptionHelper.SetEncryptionKey(encryptionKey);

        // --- DATABASE CONNECTION ---
        var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                 ?? "Server=hildur.ucn.dk;Database=DMA-CSD-V23_10481979;User Id=DMA-CSD-V23_10481979;Password=Password1!;Encrypt=False;";

        if (string.IsNullOrEmpty(dbConnectionString))
        {
            Console.WriteLine("DB_CONNECTION_STRING is missing or empty. Using fallback local connection string.");
            dbConnectionString = "Server=localhost,1433;Database=Overblikplus_Dev;User Id=sa;Password=reallyStrongPwd123;Encrypt=False;";
        }
        else
        {
            Console.WriteLine($"DB_CONNECTION_STRING: {dbConnectionString}");
        }

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(dbConnectionString));

        // --- IDENTITY CONFIGURATION ---
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

        // --- JWT CONFIGURATION ---
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                        ?? builder.Configuration["Jwt:Issuer"]
                        ?? "https://overblikplus.dk";

        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                          ?? builder.Configuration["Jwt:Audience"]
                          ?? "https://overblikplus.dk";

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                     ?? builder.Configuration["Jwt:Key"]
                     ?? "MyVeryStrongSecretKeyForJWT1234567890";

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
                policy => policy.WithOrigins("https://yellow-ocean-0f63e7903.4.azurestaticapps.net", "http://localhost:5226", "https://overblikplus.dk" )
                    .AllowAnyMethod()
                    .AllowAnyHeader());
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
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();
        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.Run();
    }
}