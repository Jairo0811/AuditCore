namespace AuditCore.Application.Features.Auth.Models;

public sealed record LoginRequest(
    string Email,
    string Password);
