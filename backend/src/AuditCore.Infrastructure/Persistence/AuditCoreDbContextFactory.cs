using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuditCore.Infrastructure.Persistence;

public sealed class AuditCoreDbContextFactory
    : IDesignTimeDbContextFactory<AuditCoreDbContext>
{
    public AuditCoreDbContext CreateDbContext(string[] args)
    {
        var apiPath = ResolveApiPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile(
                "appsettings.json",
                optional: true,
                reloadOnChange: false)
            .AddJsonFile(
                "appsettings.Development.json",
                optional: false,
                reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "La cadena de conexión 'DefaultConnection' no está configurada.");

        var optionsBuilder =
            new DbContextOptionsBuilder<AuditCoreDbContext>();

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlServerOptions =>
                sqlServerOptions.EnableRetryOnFailure());

        return new AuditCoreDbContext(
            optionsBuilder.Options);
    }

    private static string ResolveApiPath()
    {
        var currentDirectory =
            Directory.GetCurrentDirectory();

        if (File.Exists(
            Path.Combine(
                currentDirectory,
                "appsettings.Development.json")))
        {
            return currentDirectory;
        }

        var apiFromBackend = Path.Combine(
            currentDirectory,
            "src",
            "AuditCore.Api");

        if (File.Exists(
            Path.Combine(
                apiFromBackend,
                "appsettings.Development.json")))
        {
            return apiFromBackend;
        }

        var directory =
            new DirectoryInfo(currentDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "AuditCore.Api");

            if (File.Exists(
                Path.Combine(
                    candidate,
                    "appsettings.Development.json")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "No se pudo localizar el directorio de AuditCore.Api.");
    }
}