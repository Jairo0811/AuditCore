using AuditCore.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Api.IntegrationTests.Middlewares;

public sealed class GlobalExceptionMiddlewareTests
{
    [Fact]
    public void UnknownDatabaseUpdateError_ShouldBeInternalErrorWithoutLeakingDetails()
    {
        var exception = new DbUpdateException(
            "An error occurred while saving the entity changes.",
            new Exception("Cannot insert NULL into column 'Secret', table 'AuditCoreDb.dbo.Users'."));

        var (status, detail) = GlobalExceptionMiddleware.Classify(exception);

        Assert.Equal(StatusCodes.Status500InternalServerError, status);
        Assert.DoesNotContain("AuditCoreDb", detail);
        Assert.DoesNotContain("saving the entity", detail);
    }

    [Fact]
    public void ConcurrencyError_ShouldBeConflict()
    {
        var (status, _) = GlobalExceptionMiddleware.Classify(new DbUpdateConcurrencyException("concurrency"));

        Assert.Equal(StatusCodes.Status409Conflict, status);
    }

    [Theory]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(FileNotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status403Forbidden)]
    [InlineData(typeof(NullReferenceException), StatusCodes.Status500InternalServerError)]
    public void KnownExceptions_ShouldMapToExpectedStatus(Type exceptionType, int expectedStatus)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "detalle")!;

        var (status, _) = GlobalExceptionMiddleware.Classify(exception);

        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void FileNotFound_ShouldNotExposeStoragePaths()
    {
        var (_, detail) = GlobalExceptionMiddleware.Classify(new FileNotFoundException("missing", "/var/lib/auditcore/evidence/a.txt"));

        Assert.DoesNotContain("/var/lib", detail);
    }
}
