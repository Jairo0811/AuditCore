using System.Security.Claims;
using AuditCore.Application.Features.Auth;
using AuditCore.Application.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using AuditCoreLoginRequest = AuditCore.Application.Features.Auth.Models.LoginRequest;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(AuditCoreLoginRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await _authService.LoginAsync(request, cancellationToken)); }
        catch (UnauthorizedAccessException) { return Unauthorized(new { message = "Credenciales inválidas." }); }
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await _authService.RefreshAsync(request, cancellationToken)); }
        catch (UnauthorizedAccessException) { return Unauthorized(new { message = "Refresh token inválido o expirado." }); }
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        email = User.FindFirstValue(ClaimTypes.Email),
        name = User.Identity?.Name,
        organizationId = User.FindFirstValue("organization_id"),
        roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value),
        permissions = User.FindAll("permission").Select(claim => claim.Value)
    });
}
