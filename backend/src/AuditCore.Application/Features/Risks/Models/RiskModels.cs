using AuditCore.Domain.Entities;

namespace AuditCore.Application.Features.Risks.Models;

public sealed record RiskDto(
    Guid Id,
    Guid AuditId,
    string AuditCode,
    string Code,
    string Title,
    string? Description,
    int Probability,
    int Impact,
    int Score,
    RiskLevel Level,
    string? Treatment,
    Guid? OwnerUserId,
    string? OwnerName,
    RiskStatus Status,
    bool IsActive);

public sealed record CreateRiskRequest(
    Guid AuditId,
    string Title,
    string? Description,
    int Probability,
    int Impact,
    string? Treatment,
    Guid? OwnerUserId);

public sealed record UpdateRiskRequest(
    string Title,
    string? Description,
    int Probability,
    int Impact,
    string? Treatment,
    Guid? OwnerUserId);
