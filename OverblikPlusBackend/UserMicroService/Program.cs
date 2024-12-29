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


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.ApplicationInsights(
        Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING"), 
        TelemetryConverter.Traces)
    .CreateLogger();


builder.Host.UseSerilog();


string encryptionKey = builder.Configuration.GetSection("EncryptionSettings:EncryptionKey").Value;
if (string.IsNullOrEmpty(encryptionKey))
{
    throw new InvalidOperationException("Encryption key is missing in appsettings.");
}
EncryptionHelper.SetEncryptionKey(encryptionKey);

var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? 
                          "Server=localhost,1433;Database=Overblikplus_Dev;User Id=sa;Password=reallyStrongPwd123;Encrypt=False;";


builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();


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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOverblikPlus",
        policy => policy.WithOrigins("https://overblikplus.dk", "http://localhost:5226", "https://yellow-ocean-0f63e7903.4.azurestaticapps.net")
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



builder.Services.AddSingleton<ILoggerService, LoggerService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); 
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection(); 
}

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseCors("AllowOverblikPlus");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
