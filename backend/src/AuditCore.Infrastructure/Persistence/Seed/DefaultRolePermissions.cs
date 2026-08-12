namespace AuditCore.Infrastructure.Persistence.Seed;

public static class DefaultRolePermissions
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Matrix =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [DefaultRoles.SuperAdmin] = DefaultPermissions.SuperAdmin,
            [DefaultRoles.OrganizationAdmin] =
            [
                "ORGANIZATIONS.VIEW",
                "ORGANIZATIONS.MANAGE",
                "USERS.VIEW",
                "USERS.MANAGE",
                "ROLES.VIEW",
                "AUDITS.VIEW",
                "AUDITS.CREATE",
                "AUDITS.UPDATE",
                "FRAMEWORKS.VIEW",
                "RISKS.VIEW",
                "FINDINGS.VIEW",
                "EVIDENCE.VIEW",
                "ACTION_PLANS.VIEW",
                "REPORTS.VIEW",
                "REPORTS.EXPORT"
            ],
            [DefaultRoles.AuditManager] =
            [
                "AUDITS.VIEW",
                "AUDITS.CREATE",
                "AUDITS.UPDATE",
                "AUDITS.EXECUTE",
                "AUDITS.CLOSE",
                "FRAMEWORKS.VIEW",
                "RISKS.VIEW",
                "RISKS.MANAGE",
                "FINDINGS.VIEW",
                "FINDINGS.MANAGE",
                "EVIDENCE.VIEW",
                "EVIDENCE.MANAGE",
                "ACTION_PLANS.VIEW",
                "ACTION_PLANS.MANAGE",
                "REPORTS.VIEW",
                "REPORTS.EXPORT"
            ],
            [DefaultRoles.Auditor] =
            [
                "AUDITS.VIEW",
                "AUDITS.EXECUTE",
                "FRAMEWORKS.VIEW",
                "RISKS.VIEW",
                "RISKS.MANAGE",
                "FINDINGS.VIEW",
                "FINDINGS.MANAGE",
                "EVIDENCE.VIEW",
                "EVIDENCE.MANAGE",
                "ACTION_PLANS.VIEW",
                "ACTION_PLANS.MANAGE",
                "REPORTS.VIEW",
                "REPORTS.EXPORT"
            ],
            [DefaultRoles.Auditee] =
            [
                "AUDITS.VIEW",
                "RISKS.VIEW",
                "FINDINGS.VIEW",
                "EVIDENCE.VIEW",
                "ACTION_PLANS.VIEW",
                "ACTION_PLANS.MANAGE",
                "REPORTS.VIEW"
            ],
            [DefaultRoles.RiskOwner] =
            [
                "AUDITS.VIEW",
                "RISKS.VIEW",
                "RISKS.MANAGE",
                "FINDINGS.VIEW",
                "ACTION_PLANS.VIEW",
                "ACTION_PLANS.MANAGE",
                "REPORTS.VIEW"
            ],
            [DefaultRoles.Viewer] =
            [
                "ORGANIZATIONS.VIEW",
                "AUDITS.VIEW",
                "FRAMEWORKS.VIEW",
                "RISKS.VIEW",
                "FINDINGS.VIEW",
                "EVIDENCE.VIEW",
                "ACTION_PLANS.VIEW",
                "REPORTS.VIEW"
            ]
        };
}
