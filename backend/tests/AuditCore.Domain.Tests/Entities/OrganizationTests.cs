using AuditCore.Domain.Entities;

namespace AuditCore.Domain.Tests.Entities;

public sealed class OrganizationTests
{
    [Fact]
    public void Constructor_ShouldNormalizeValues_AndActivateOrganization()
    {
        var organization = new Organization(
            "  Empresa Demo  ",
            " demo ",
            "  Organización de prueba  ");

        Assert.Equal("Empresa Demo", organization.Name);
        Assert.Equal("DEMO", organization.Code);
        Assert.Equal("Organización de prueba", organization.Description);
        Assert.True(organization.IsActive);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Organization("", "DEMO"));
    }

    [Fact]
    public void Deactivate_ShouldSetOrganizationAsInactive()
    {
        var organization = new Organization("Empresa Demo", "DEMO");

        organization.Deactivate();

        Assert.False(organization.IsActive);
    }
}
