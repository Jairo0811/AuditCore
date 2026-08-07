using AuditCore.Domain.Entities;

namespace AuditCore.Domain.Tests.Entities;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveToken()
    {
        var token = new RefreshToken(
            Guid.NewGuid(),
            new string('A', 64),
            DateTime.UtcNow.AddHours(1));

        Assert.True(token.IsActive);
        Assert.False(token.IsExpired);
        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void Revoke_ShouldInvalidateToken()
    {
        var token = new RefreshToken(
            Guid.NewGuid(),
            new string('A', 64),
            DateTime.UtcNow.AddHours(1));

        var replacementHash =
            new string('B', 64);

        token.Revoke(replacementHash);

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
        Assert.Equal(
            replacementHash,
            token.ReplacedByTokenHash);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExpirationIsNotFuture()
    {
        Assert.Throws<ArgumentException>(() =>
            new RefreshToken(
                Guid.NewGuid(),
                new string('A', 64),
                DateTime.UtcNow.AddMinutes(-1)));
    }
}
