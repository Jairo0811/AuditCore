namespace AuditCore.Infrastructure.Persistence.Seed;

public static class DefaultPermissions
{
    public static readonly IReadOnlyCollection<PermissionSeed> All =
    [
        new("ORGANIZATIONS.VIEW", "Ver organizaciones"),
        new("ORGANIZATIONS.MANAGE", "Administrar organizaciones"),
        new("USERS.VIEW", "Ver usuarios"),
        new("USERS.MANAGE", "Administrar usuarios"),
        new("ROLES.VIEW", "Ver roles"),
        new("ROLES.MANAGE", "Administrar roles"),
        new("AUDITS.VIEW", "Ver auditorías"),
        new("AUDITS.CREATE", "Crear auditorías"),
        new("AUDITS.UPDATE", "Modificar auditorías"),
        new("AUDITS.EXECUTE", "Ejecutar auditorías"),
        new("AUDITS.CLOSE", "Cerrar auditorías"),
        new("FRAMEWORKS.VIEW", "Ver marcos y controles"),
        new("FRAMEWORKS.MANAGE", "Administrar marcos y controles"),
        new("EVIDENCE.VIEW", "Ver evidencias"),
        new("EVIDENCE.MANAGE", "Administrar evidencias"),
        new("FINDINGS.VIEW", "Ver hallazgos"),
        new("FINDINGS.MANAGE", "Administrar hallazgos"),
        new("RISKS.VIEW", "Ver riesgos"),
        new("RISKS.MANAGE", "Administrar riesgos"),
        new("ACTION_PLANS.VIEW", "Ver planes de acción"),
        new("ACTION_PLANS.MANAGE", "Administrar planes de acción"),
        new("REPORTS.VIEW", "Ver reportes"),
        new("REPORTS.EXPORT", "Exportar reportes"),
        new("SETTINGS.MANAGE", "Administrar configuración")
    ];

    public static IReadOnlyCollection<string> SuperAdmin => All.Select(permission => permission.Code).ToArray();
}

public sealed record PermissionSeed(string Code, string Name);
