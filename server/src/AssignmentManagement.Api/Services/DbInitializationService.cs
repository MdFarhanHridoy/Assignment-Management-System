using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Infrastructure.Data;
using AssignmentManagement.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AssignmentManagement.Api.Services;

public class DbInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DbInitializationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var logger = scopedServices.GetRequiredService<ILogger<DbInitializationService>>();

        try
        {
            var db = scopedServices.GetRequiredService<AppDbContext>();
            var passwordHasher = scopedServices.GetRequiredService<IPasswordHasher>();

            logger.LogInformation("Applying pending EF Core migrations...");
            await db.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Seeding initial data...");
            await DbSeeder.SeedAsync(db, passwordHasher, cancellationToken);

            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
