using AuditCore.Application.Common.Security;

namespace AuditCore.Application.Tests.Security;

public sealed class PasswordPolicyTests
{
    [Theory]
    [InlineData("AuditCore123..")]
    [InlineData("Integration-Test-2026")]
    [InlineData("abcdefghi1")]
    public void IsValid_ShouldAcceptPasswordsMeetingPolicy(string password)
    {
        Assert.True(PasswordPolicy.IsValid(password));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("          ")]
    [InlineData("abc12")]
    [InlineData("abcdefghij")]
    [InlineData("1234567890")]
    public void IsValid_ShouldRejectWeakPasswords(string? password)
    {
        Assert.False(PasswordPolicy.IsValid(password));
    }

    [Fact]
    public void IsValid_ShouldRejectPasswordsLongerThanMaximum()
    {
        var password = new string('a', PasswordPolicy.MaximumLength) + "1";

        Assert.False(PasswordPolicy.IsValid(password));
    }

    [Fact]
    public void EnsureValid_ShouldThrowArgumentExceptionWithReadableMessage()
    {
        var exception = Assert.Throws<ArgumentException>(() => PasswordPolicy.EnsureValid("corta"));

        Assert.Equal(PasswordPolicy.Description, exception.Message);
    }
}
