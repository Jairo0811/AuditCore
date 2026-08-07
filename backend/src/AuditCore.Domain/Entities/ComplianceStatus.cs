namespace AuditCore.Domain.Entities;

public enum ComplianceStatus
{
    NotEvaluated = 1,
    NonCompliant = 2,
    PartiallyCompliant = 3,
    Compliant = 4,
    NotApplicable = 5
}
