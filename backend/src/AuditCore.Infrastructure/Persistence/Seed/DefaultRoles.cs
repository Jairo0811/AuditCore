namespace AuditCore.Infrastructure.Persistence.Seed;

public static class DefaultRoles
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string OrganizationAdmin = "ORGANIZATION_ADMIN";
    public const string AuditManager = "AUDIT_MANAGER";
    public const string Auditor = "AUDITOR";
    public const string Auditee = "AUDITEE";
    public const string RiskOwner = "RISK_OWNER";
    public const string Viewer = "VIEWER";

    public static readonly IReadOnlyCollection<RoleSeed> All =
    [
        new(SuperAdmin, "Super Administrador", "Acceso total a la plataforma."),
        new(OrganizationAdmin, "Administrador de Organización", "Administra una organización y sus usuarios."),
        new(AuditManager, "Gestor de Auditorías", "Planifica y administra auditorías."),
        new(Auditor, "Auditor", "Ejecuta auditorías y registra evidencias y hallazgos."),
        new(Auditee, "Auditado", "Participa en procesos de auditoría y remediación."),
        new(RiskOwner, "Responsable de Riesgos", "Gestiona riesgos y planes de tratamiento."),
        new(Viewer, "Consulta", "Acceso de solo lectura.")
    ];
}

public sealed record RoleSeed(
    string Code,
    string Name,
    string Description);
