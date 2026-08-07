namespace AuditCore.Application.Common.Security;

public static class PermissionCodes
{
    public const string OrganizationsView = "ORGANIZATIONS.VIEW";
    public const string OrganizationsManage = "ORGANIZATIONS.MANAGE";

    public const string UsersView = "USERS.VIEW";
    public const string UsersManage = "USERS.MANAGE";

    public const string RolesView = "ROLES.VIEW";
    public const string RolesManage = "ROLES.MANAGE";

    public const string AuditsView = "AUDITS.VIEW";
    public const string AuditsCreate = "AUDITS.CREATE";
    public const string AuditsUpdate = "AUDITS.UPDATE";
    public const string AuditsExecute = "AUDITS.EXECUTE";
    public const string AuditsClose = "AUDITS.CLOSE";

    public const string EvidenceView = "EVIDENCE.VIEW";
    public const string EvidenceManage = "EVIDENCE.MANAGE";

    public const string FindingsView = "FINDINGS.VIEW";
    public const string FindingsManage = "FINDINGS.MANAGE";

    public const string RisksView = "RISKS.VIEW";
    public const string RisksManage = "RISKS.MANAGE";

    public const string ActionPlansView = "ACTION_PLANS.VIEW";
    public const string ActionPlansManage = "ACTION_PLANS.MANAGE";

    public const string ReportsView = "REPORTS.VIEW";
    public const string ReportsExport = "REPORTS.EXPORT";

    public const string SettingsManage = "SETTINGS.MANAGE";

    public static readonly IReadOnlyCollection<string> All =
    [
        OrganizationsView,
        OrganizationsManage,
        UsersView,
        UsersManage,
        RolesView,
        RolesManage,
        AuditsView,
        AuditsCreate,
        AuditsUpdate,
        AuditsExecute,
        AuditsClose,
        EvidenceView,
        EvidenceManage,
        FindingsView,
        FindingsManage,
        RisksView,
        RisksManage,
        ActionPlansView,
        ActionPlansManage,
        ReportsView,
        ReportsExport,
        SettingsManage
    ];
}
