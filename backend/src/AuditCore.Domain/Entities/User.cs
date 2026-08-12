using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class User : BaseAuditableEntity
{
    private User()
    {
    }

    public User(
        Guid organizationId,
        string firstName,
        string lastName,
        string email,
        string passwordHash)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "La organización es obligatoria.",
                nameof(organizationId));
        }

        OrganizationId = organizationId;
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetPasswordHash(passwordHash);

        IsActive = true;
    }

    public Guid OrganizationId { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public bool IsLocked { get; private set; }

    public DateTime? LastLoginAtUtc { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    public string FullName => $"{FirstName} {LastName}".Trim();

    public void UpdateProfile(
        string firstName,
        string lastName,
        string email)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
    }

    public void ChangePassword(string passwordHash)
    {
        SetPasswordHash(passwordHash);
    }

    public void RegisterLogin()
    {
        LastLoginAtUtc = DateTime.UtcNow;
    }

    public void Lock() => IsLocked = true;

    public void Unlock() => IsLocked = false;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void SetFirstName(string firstName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        FirstName = firstName.Trim();
    }

    private void SetLastName(string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        LastName = lastName.Trim();
    }

    private void SetEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Email = email.Trim().ToLowerInvariant();
    }

    private void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash.Trim();
    }
}
