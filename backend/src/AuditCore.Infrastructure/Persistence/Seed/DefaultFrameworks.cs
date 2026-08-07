namespace AuditCore.Infrastructure.Persistence.Seed;

public static class DefaultFrameworks
{
    public static readonly IReadOnlyCollection<FrameworkSeed> All =
    [
        new(
            "COBIT 2019",
            "COBIT",
            "2019",
            "Marco de referencia inicial de AuditCore. El catálogo detallado de controles debe configurarse con contenido autorizado/licenciado por la organización.")
    ];
}

public sealed record FrameworkSeed(
    string Name,
    string Code,
    string Version,
    string? Description);
