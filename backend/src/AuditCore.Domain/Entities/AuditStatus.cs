namespace AuditCore.Domain.Entities;

public enum AuditStatus
{
    Draft = 1,
    Planned = 2,
    InProgress = 3,
    Completed = 4,
    Closed = 5,
    Cancelled = 6
}
