using AuditCore.Domain.Entities;

namespace AuditCore.Application.Features.Findings.Models;

public sealed record FindingDto(
    Guid Id,
    Guid AuditId,
    string AuditCode,
    Guid? RiskId,
    string? RiskCode,
    string Code,
    string Title,
    string Condition,
    string Criteria,
    string? Cause,
    string? Effect,
    string? Recommendation,
    FindingSeverity Severity,
    Guid? ResponsibleUserId,
    string? ResponsibleName,
    DateTime? DueDateUtc,
    FindingStatus Status,
    bool IsActive);

public sealed record CreateFindingRequest(
    Guid AuditId,
    Guid? RiskId,
    string Code,
    string Title,
    string Condition,
    string Criteria,
    string? Cause,
    string? Effect,
    string? Recommendation,
    FindingSeverity Severity,
    Guid? ResponsibleUserId,
    DateTime? DueDateUtc);

public sealed record UpdateFindingRequest(
    Guid? RiskId,
    string Code,
    string Title,
    string Condition,
    string Criteria,
    string? Cause,
    string? Effect,
    string? Recommendation,
    FindingSeverity Severity,
    Guid? ResponsibleUserId,
    DateTime? DueDateUtc);
