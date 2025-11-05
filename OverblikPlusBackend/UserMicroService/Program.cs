using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
using UserMicroService.Hubs;
using SeedData;

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

        if (builder.Environment.IsDevelopment())
        {
            logger.LogInfo($"UserMicroService starting in {builder.Environment.EnvironmentName} environment");
        }
        
        var dbConnectionString = builder.Configuration.GetConnectionString("DBConnectionString");
        if (string.IsNullOrEmpty(dbConnectionString))
        {
            logger.LogError("DB_CONNECTION_STRING is missing or empty.", new InvalidOperationException("Missing connection string"));
            throw new InvalidOperationException("DB_CONNECTION_STRING is missing or empty.");
        }
        
        if (builder.Environment.IsDevelopment())
        {
            logger.LogInfo("Configuration validated successfully - Database connection string found");
        }

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(dbConnectionString, x => x.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));
      
        // Robust fallback - first non-empty value
        string FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? "";

        var encryptionKeyBase64 = FirstNonEmpty(
            builder.Configuration["EncryptionSettings:EncryptionKey"],
            builder.Configuration["Encryption:Key"],
            Environment.GetEnvironmentVariable("ENCRYPTION_KEY")
        );
        
        if (string.IsNullOrWhiteSpace(encryptionKeyBase64))
        {
            logger.LogError("Encryption key is missing from all sources.", new InvalidOperationException("Missing encryption key"));
            throw new InvalidOperationException("Encryption key is missing from all sources.");
        }
        
        if (builder.Environment.IsDevelopment())
        {
            logger.LogInfo("Encryption key validated successfully");
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

        if (builder.Environment.IsDevelopment())
        {
            logger.LogInfo($"JWT Configuration - Issuer: {jwtIssuer ?? "NULL"}, Audience: {jwtAudience ?? "NULL"}, Key Length: {jwtKey?.Length ?? -1}");
        }
        
        if (string.IsNullOrEmpty(jwtKey))
        {
            logger.LogError("JWT Key is missing from configuration.", new InvalidOperationException("Missing JWT key"));
            throw new InvalidOperationException("JWT Key is required but was not found in configuration.");
        }

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""))
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
            // Local development policy
            options.AddPolicy("AllowLocalDev",
                policy =>
                {
                    policy.WithOrigins("http://localhost:5226", "http://localhost:5003")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetPreflightMaxAge(TimeSpan.FromHours(1));
                });
            
            options.AddPolicy("AllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins(
                            "https://nice-wave-08dd97903.1.azurestaticapps.net",  // PROD Static Web App
                            "https://overblikplus.dk",                              // PROD Custom Domain
                            "https://witty-meadow-0c52c9003.2.azurestaticapps.net", // DEV Static Web App
                            "http://localhost:5226",                                 // Local Development (Rider Frontend)
                            "http://localhost:5003",                                 // Docker Development (Docker Frontend)
                            "http://localhost:5004"                                  // Rider UserService (for testing)
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
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserMicroService API", Version = "v1" });
            
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
        
        // CORS skal være mellem UseRouting og UseAuthentication/UseAuthorization
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserMicroService API V1");
            c.RoutePrefix = "swagger";
        });

        app.UseSerilogRequestLogging();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapHub<ChatHub>("/chatHub");

        app.MapControllers()
            .RequireCors(app.Environment.IsDevelopment() ? "AllowLocalDev" : "AllowSpecificOrigins");

        var seeder = new DatabaseSeeder<UserDbContext>(
            logger,
            app.Environment,
            async (serviceProvider, context) =>
            {
                await Helpers.UserSeeder.SeedUsersAsync(serviceProvider, logger);
            });
        
        await seeder.SeedAsync(app.Services);
        
        logger.LogInfo($"[UserMicroService] About to start application in {app.Environment.EnvironmentName} mode.");
        
        try
        {
            logger.LogInfo("[UserMicroService] Calling app.RunAsync()...");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogError($"[UserMicroService] FATAL ERROR during app.RunAsync(): {ex.Message}", ex);
            logger.LogError($"[UserMicroService] Stack trace: {ex.StackTrace}", ex);
            throw;
        }
    }

}