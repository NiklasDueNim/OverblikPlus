using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OverblikPlus.Shared.Interfaces;

namespace SeedData;

public class DatabaseSeeder<TContext> where TContext : DbContext
{
    private readonly ILoggerService _logger;
    private readonly IHostEnvironment _environment;
    private readonly Func<IServiceProvider, TContext, Task>? _seedDataCallback;

    public DatabaseSeeder(
        ILoggerService logger, 
        IHostEnvironment environment,
        Func<IServiceProvider, TContext, Task>? seedDataCallback = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _seedDataCallback = seedDataCallback;
    }

    public async Task SeedAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            var conn = context.Database.GetDbConnection();
            
            _logger.LogInfo($"DB target: {conn.DataSource}/{conn.Database}");
            
            await MigrateDatabaseAsync(context);
            
            if (_seedDataCallback != null)
            {
                await _seedDataCallback(scope.ServiceProvider, context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Database seeding failed: {ex.Message}", ex);
        }
    }

    private async Task MigrateDatabaseAsync(TContext context)
    {
        try
        {
            await context.Database.MigrateAsync();
            var contextName = typeof(TContext).Name;
            _logger.LogInfo($"[{contextName}] Database migrations completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"DB migration failed at startup - continuing without migration: {ex.Message}", ex);
            
            try
            {
                await context.Database.EnsureCreatedAsync();
                var contextName = typeof(TContext).Name;
                _logger.LogInfo($"[{contextName}] Database ensured created.");
            }
            catch (Exception ensureEx)
            {
                _logger.LogError($"EnsureCreated failed: {ensureEx.Message}", ensureEx);
                var contextName = typeof(TContext).Name;
                _logger.LogInfo($"[{contextName}] Continuing despite database setup failure.");
            }
        }
    }
}
