using AuditCore.Application.Common.Security;

namespace AuditCore.Application.Tests.Security;

public sealed class PermissionCodesTests
{
    [Fact]
    public void All_ShouldContainUniquePermissionCodes()
    {
        var distinctCount = PermissionCodes.All
            .Distinct(StringComparer.Ordinal)
            .Count();

        Assert.Equal(
            PermissionCodes.All.Count,
            distinctCount);
    }

    [Fact]
    public void All_ShouldContainCoreSecurityPermissions()
    {
        Assert.Contains(
            PermissionCodes.UsersView,
            PermissionCodes.All);

        Assert.Contains(
            PermissionCodes.UsersManage,
            PermissionCodes.All);

        Assert.Contains(
            PermissionCodes.RolesView,
            PermissionCodes.All);

        Assert.Contains(
            PermissionCodes.RolesManage,
            PermissionCodes.All);
    }

    [Fact]
    public void All_ShouldContainAuditDomainPermissions()
    {
        Assert.Contains(
            PermissionCodes.AuditsView,
            PermissionCodes.All);

        Assert.Contains(
            PermissionCodes.FindingsView,
            PermissionCodes.All);

        Assert.Contains(
            PermissionCodes.RisksView,
            PermissionCodes.All);

        Assert.Contains(
            PermissionCodes.ActionPlansView,
            PermissionCodes.All);
    }
}
