using System.Globalization;
using System.Text;

namespace AuditCore.Infrastructure.Services;

internal static class EntityCodeGenerator
{
    private static readonly HashSet<string> IgnoredWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "DE", "DEL", "LA", "LAS", "EL", "LOS", "Y", "E", "OF", "THE", "AND"
    };

    public static string BuildPrefix(string name, string fallback, int maxLength = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var words = RemoveDiacritics(name)
            .ToUpperInvariant()
            .Split([' ', '-', '_', '.', ',', '/', '\\', '(', ')'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(KeepLettersAndDigits)
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToArray();

        var meaningfulWords = words
            .Where(word => !IgnoredWords.Contains(word))
            .ToArray();

        if (meaningfulWords.Length >= 2)
        {
            var acronym = string.Concat(meaningfulWords.Select(word => word[0]));
            return acronym[..Math.Min(maxLength, acronym.Length)];
        }

        var source = meaningfulWords.FirstOrDefault() ?? words.FirstOrDefault() ?? fallback;
        return source[..Math.Min(maxLength, source.Length)];
    }

    private static string RemoveDiacritics(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string KeepLettersAndDigits(string value) =>
        new(value.Where(char.IsLetterOrDigit).ToArray());
}
