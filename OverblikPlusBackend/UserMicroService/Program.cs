using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserMicroService.DataAccess;
using UserMicroService.Entities;
using UserMicroService.Helpers;
using UserMicroService.Services;
using UserMicroService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- LOAD ENVIRONMENT VARIABLES ---
var configuration = builder.Configuration;

// ---- ENCRYPTION CONFIGURATION ----
// Hent Encryption Key fra miljøvariabler eller appsettings.json
string encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") 
                       ?? configuration["EncryptionSettings:EncryptionKey"];

// Valider længden af nøglen
if (string.IsNullOrEmpty(encryptionKey) || 
    (encryptionKey.Length != 16 && encryptionKey.Length != 24 && encryptionKey.Length != 32))
{
    throw new InvalidOperationException("Encryption key is missing or has invalid length. It must be 16, 24, or 32 characters long.");
}

// Sæt nøglen
EncryptionHelper.SetEncryptionKey(encryptionKey);


// ---- DATABASE CONFIGURATION ----
var connectionString = configuration["DB_CONNECTION_STRING"];
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---- IDENTITY CONFIGURATION ----
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

// ---- JWT AUTHENTICATION ----
var jwtKey = configuration["JWT_KEY"];
var jwtIssuer = configuration["JWT_ISSUER"];
var jwtAudience = configuration["JWT_AUDIENCE"];

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

// ---- CORS CONFIGURATION ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOverblikPlus",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ---- SERVICES ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ---- BUILD APPLICATION ----
var app = builder.Build();

// ---- CONFIGURE MIDDLEWARE ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowOverblikPlus");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
