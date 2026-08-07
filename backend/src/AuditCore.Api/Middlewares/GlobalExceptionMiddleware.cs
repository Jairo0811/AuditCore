using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Api.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled request error. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteProblemAsync(context, exception);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var status = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            FileNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var detail = status == StatusCodes.Status500InternalServerError
            ? "Ocurrió un error interno inesperado."
            : exception.Message;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = status switch
            {
                400 => "Solicitud inválida",
                403 => "Acceso denegado",
                404 => "Recurso no encontrado",
                409 => "Conflicto de negocio",
                _ => "Error interno"
            },
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
