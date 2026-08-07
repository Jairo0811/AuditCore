using AuditCore.Domain.Entities;

namespace AuditCore.Domain.Tests.Entities;

public sealed class UserTests
{
    [Fact]
    public void Constructor_ShouldNormalizeUserData()
    {
        var organizationId = Guid.NewGuid();

        var user = new User(
            organizationId,
            "  Francis  ",
            "  Matías  ",
            "  TEST@AUDITCORE.COM  ",
            "hashed-password");

        Assert.Equal(organizationId, user.OrganizationId);
        Assert.Equal("Francis", user.FirstName);
        Assert.Equal("Matías", user.LastName);
        Assert.Equal("test@auditcore.com", user.Email);
        Assert.Equal("Francis Matías", user.FullName);
        Assert.True(user.IsActive);
        Assert.False(user.IsLocked);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOrganizationIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            new User(
                Guid.Empty,
                "Francis",
                "Matías",
                "test@auditcore.com",
                "hashed-password"));
    }

    [Fact]
    public void LockAndUnlock_ShouldChangeLockState()
    {
        var user = new User(
            Guid.NewGuid(),
            "Francis",
            "Matías",
            "test@auditcore.com",
            "hashed-password");

        user.Lock();

        Assert.True(user.IsLocked);

        user.Unlock();

        Assert.False(user.IsLocked);
    }
}
