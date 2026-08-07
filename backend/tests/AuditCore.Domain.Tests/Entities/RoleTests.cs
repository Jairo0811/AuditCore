using AuditCore.Domain.Entities;

namespace AuditCore.Domain.Tests.Entities;

public sealed class RoleTests
{
    [Fact]
    public void Constructor_ShouldNormalizeRoleData()
    {
        var role = new Role(
            "  Administrador  ",
            " admin ",
            "  Acceso administrativo  ");

        Assert.Equal("Administrador", role.Name);
        Assert.Equal("ADMIN", role.Code);
        Assert.Equal("Acceso administrativo", role.Description);
        Assert.True(role.IsActive);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCodeIsEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Role("Administrador", ""));
    }
}
