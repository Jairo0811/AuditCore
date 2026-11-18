using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
            var (status, detail) = Classify(exception);

            if (status >= StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled request error. TraceId: {TraceId}", context.TraceIdentifier);
            else
                _logger.LogWarning(exception, "Request rejected with status {Status}. TraceId: {TraceId}", status, context.TraceIdentifier);

            if (context.Response.HasStarted) throw;
            await WriteProblemAsync(context, status, detail);
        }
    }

    /// <summary>
    /// Traduce excepciones conocidas a códigos HTTP. Los errores de base de datos solo se convierten en 409
    /// cuando corresponden a violaciones de integridad previsibles; el resto se trata como error interno
    /// para no ocultar fallos reales. Nunca se expone el mensaje interno de SQL Server.
    /// </summary>
    public static (int Status, string Detail) Classify(Exception exception)
    {
        const string internalError = "Ocurrió un error interno inesperado.";

        return exception switch
        {
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
                "El registro fue modificado por otro usuario. Actualiza la información e inténtalo nuevamente."),
            DbUpdateException dbUpdate => ClassifyDatabaseError(dbUpdate) is { } message
                ? (StatusCodes.Status409Conflict, message)
                : (StatusCodes.Status500InternalServerError, internalError),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, exception.Message),
            FileNotFoundException => (StatusCodes.Status404NotFound, "El recurso solicitado no existe."),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, internalError)
        };
    }

    private static string? ClassifyDatabaseError(DbUpdateException exception)
    {
        var sqlException = exception.InnerException as SqlException
            ?? exception.InnerException?.InnerException as SqlException;

        return sqlException?.Number switch
        {
            2601 or 2627 => "Ya existe un registro con los mismos datos únicos.",
            547 => "La operación no puede completarse porque el registro está relacionado con otra información.",
            _ => null
        };
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string detail)
    {
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

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
