using System.Text.RegularExpressions;

namespace DuncanLaud.Domain.Services;

/// <summary>
/// Pure business rule validation for person fields. No external dependencies.
/// </summary>
public static class PersonValidator
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 100;

    /// <summary>Allows only A-Za-z0-9 characters.</summary>
    private static readonly Regex ValidCharsRegex =
        new(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);

    /// <summary>
    /// Removes every character that is not A-Za-z0-9.
    /// </summary>
    public static string Sanitize(string? input)
        => string.IsNullOrEmpty(input)
            ? string.Empty
            : Regex.Replace(input, @"[^A-Za-z0-9]", string.Empty);

    /// <summary>
    /// Returns true when the name contains only A-Za-z0-9 and is within the
    /// allowed length range. Pass <c>required = false</c> for optional fields
    /// (null / empty is then accepted).
    /// </summary>
    public static bool IsValidName(string? name, bool required = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            return !required;

        var trimmed = name.Trim();
        return trimmed.Length >= MinNameLength
            && trimmed.Length <= MaxNameLength
            && ValidCharsRegex.IsMatch(trimmed);
    }

    public static bool IsValidBirthDate(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return birthDate < today && birthDate.Year >= 1900;
    }
}
