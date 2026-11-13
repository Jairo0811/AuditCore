namespace AuditCore.Application.Common.Security;

/// <summary>
/// Políticas compuestas que no corresponden a un único permiso.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Consulta mínima de usuarios (id, nombre y correo) necesaria para asignar responsables,
    /// auditores líderes y evaluadores. Se concede a quien puede gestionar usuarios o a quien
    /// gestiona recursos que requieren seleccionar un responsable.
    /// </summary>
    public const string UserLookup = "USERS.LOOKUP";

    public static readonly IReadOnlyCollection<string> UserLookupPermissions =
    [
        PermissionCodes.UsersView,
        PermissionCodes.AuditsUpdate,
        PermissionCodes.AuditsExecute,
        PermissionCodes.RisksManage,
        PermissionCodes.FindingsManage,
        PermissionCodes.ActionPlansManage
    ];
}
