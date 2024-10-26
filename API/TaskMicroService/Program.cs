using Microsoft.EntityFrameworkCore;
using TaskMicroService.DataAccess;
using TaskMicroService.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrer DbContext og andre services
builder.Services.AddDbContext<TaskDbContext>(options =>
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
builder.Services.AddAutoMapper(typeof(Program));  // Tilføj AutoMapper for mapping
builder.Services.AddScoped<ITaskService, TaskService>();  // Registrer TaskMicroService
builder.Services.AddScoped<ITaskStepService, TaskStepService>();

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