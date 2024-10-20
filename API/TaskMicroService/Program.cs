using TaskMicroService.DataAccess;
using Microsoft.EntityFrameworkCore;
using TaskMicroService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TaskDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();