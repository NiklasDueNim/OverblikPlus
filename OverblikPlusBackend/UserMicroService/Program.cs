using Microsoft.EntityFrameworkCore;
using UserMicroService.DataAccess;
using UserMicroService.Helpers;
using UserMicroService.Services;

var builder = WebApplication.CreateBuilder(args);

// Hent krypteringsnøglen fra appsettings
string encryptionKey = builder.Configuration.GetSection("EncryptionSettings:EncryptionKey").Value;
if (string.IsNullOrEmpty(encryptionKey))
{
    throw new InvalidOperationException("Encryption key is missing in appsettings.");
}

// Sæt krypteringsnøglen
EncryptionHelper.SetEncryptionKey(encryptionKey);

// Registrer DbContext og andre services
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

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

app.UseCors("AllowAllOrigins");

app.UseAuthorization();
app.MapControllers();
app.Run();