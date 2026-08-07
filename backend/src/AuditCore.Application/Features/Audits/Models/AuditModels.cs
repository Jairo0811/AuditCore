using AuditCore.Domain.Entities;

namespace AuditCore.Application.Features.Audits.Models;

public sealed record AuditDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string Code,
    string Title,
    string? Objective,
    string? Scope,
    Guid? LeadAuditorUserId,
    string? LeadAuditorName,
    DateTime? StartDateUtc,
    DateTime? EndDateUtc,
    AuditStatus Status,
    bool IsActive);

public sealed record CreateAuditRequest(
    Guid OrganizationId,
    string Code,
    string Title,
    string? Objective,
    string? Scope);

public sealed record UpdateAuditRequest(
    string Code,
    string Title,
    string? Objective,
    string? Scope);

public sealed record PlanAuditRequest(
    Guid LeadAuditorUserId,
    DateTime StartDateUtc,
    DateTime EndDateUtc);
