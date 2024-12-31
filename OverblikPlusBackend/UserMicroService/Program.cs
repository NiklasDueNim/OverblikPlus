using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using OverblikPlus.Shared.Interfaces;
using OverblikPlus.Shared.Logging;
using UserMicroService.DataAccess;
using UserMicroService.dto;
using UserMicroService.Entities;
using UserMicroService.Helpers;
using UserMicroService.Services;
using UserMicroService.Services.Interfaces;
using UserMicroService.Validators;

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

// --- ENCRYPTION KEY ---
// 1) Læs fra env-var, ellers fra appsettings ("EncryptionSettings:EncryptionKey")
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
// 1) Læs fra env-vars, fallback til appsettings
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "https://overblikplus.dk";  // fallback

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? builder.Configuration["Jwt:Audience"]
    ?? "https://overblikplus.dk";  // fallback

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
    ?? builder.Configuration["Jwt:Key"]
    ?? "MyVeryStrongSecretKeyForJWT1234567890"; // fallback

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

// Log JWT-variabler (debug formål)
Console.WriteLine($"Jwt:Issuer   = {jwtIssuer}");
Console.WriteLine($"Jwt:Audience = {jwtAudience}");
Console.WriteLine($"Jwt:Key      = {jwtKey}");

// --- CORS CONFIGURATION ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOverblikPlus",
        policy => policy.WithOrigins(
                "https://overblikplus.dk",
                "https://yellow-ocean-0f63e7903.4.azurestaticapps.net",
                "http://localhost:5226")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// --- SERVICES & DEPENDENCIES ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
builder.Services.AddSingleton<ILoggerService, LoggerService>();

// --- BUILD APPLICATION ---
var app = builder.Build();

// --- MIDDLEWARE & CONFIG ---
if (app.Environment.IsDevelopment())
{
    // Kun i dev
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
app.UseCors("AllowOverblikPlus");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
