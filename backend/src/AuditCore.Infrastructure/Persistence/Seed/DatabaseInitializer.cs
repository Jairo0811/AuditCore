using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Persistence.Seed;

public sealed class DatabaseInitializer
{
    private readonly AuditCoreDbContext _dbContext;
    private readonly AuditCoreSeeder _seeder;

    public DatabaseInitializer(
        AuditCoreDbContext dbContext,
        AuditCoreSeeder seeder)
    {
        _dbContext = dbContext;
        _seeder = seeder;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        await _seeder.SeedAsync(cancellationToken);
    }
}
