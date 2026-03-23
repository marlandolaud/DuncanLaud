namespace DuncanLaud.Domain.Services;

/// <summary>
/// Pure business rule validation for person fields. No external dependencies.
/// </summary>
public static class PersonValidator
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 100;

    public static bool IsValidName(string? name, bool required = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            return !required;

        var trimmed = name.Trim();
        return trimmed.Length >= MinNameLength && trimmed.Length <= MaxNameLength;
    }

    public static bool IsValidBirthDate(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return birthDate < today && birthDate.Year >= 1900;
    }
}
