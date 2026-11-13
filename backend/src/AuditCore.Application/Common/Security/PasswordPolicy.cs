namespace AuditCore.Application.Common.Security;

/// <summary>
/// Política mínima de contraseñas aplicada por el backend al crear usuarios o cambiar contraseñas.
/// El frontend replica estas reglas solo como ayuda; esta clase es la fuente de verdad.
/// </summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 10;
    public const int MaximumLength = 128;

    public const string Description =
        "La contraseña debe tener entre 10 y 128 caracteres e incluir al menos una letra y un número.";

    public static bool IsValid(string? password) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= MinimumLength
        && password.Length <= MaximumLength
        && password.Any(char.IsLetter)
        && password.Any(char.IsDigit);

    public static void EnsureValid(string? password)
    {
        if (!IsValid(password))
            throw new ArgumentException(Description);
    }
}
