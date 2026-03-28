using DuncanLaud.Domain.Services;

namespace DuncanLaud.Domain.Tests;

public class PersonValidatorTests
{
    // ── IsValidName ────────────────────────────────────

    [Fact]
    public void IsValidName_NullRequired_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName(null, required: true));

    [Fact]
    public void IsValidName_NullOptional_ReturnsTrue()
        => Assert.True(PersonValidator.IsValidName(null, required: false));

    [Fact]
    public void IsValidName_EmptyRequired_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName("", required: true));

    [Fact]
    public void IsValidName_EmptyOptional_ReturnsTrue()
        => Assert.True(PersonValidator.IsValidName("", required: false));

    [Fact]
    public void IsValidName_WhitespaceRequired_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName("   ", required: true));

    [Fact]
    public void IsValidName_WhitespaceOptional_ReturnsTrue()
        => Assert.True(PersonValidator.IsValidName("   ", required: false));

    [Fact]
    public void IsValidName_SingleChar_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName("A"));

    [Fact]
    public void IsValidName_TwoChars_ReturnsTrue()
        => Assert.True(PersonValidator.IsValidName("Ab"));

    [Fact]
    public void IsValidName_HundredChars_ReturnsTrue()
        => Assert.True(PersonValidator.IsValidName(new string('A', 100)));

    [Fact]
    public void IsValidName_HundredOneChars_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName(new string('A', 101)));

    [Fact]
    public void IsValidName_WithLeadingTrailingSpaces_TrimsBeforeCheck()
    {
        // Sanitize strips spaces before IsValidName is called; IsValidName still trims internally for the length check.
        // "  Ab  " trimmed = "Ab" (length 2) → valid
        Assert.True(PersonValidator.IsValidName("  Ab  "));
    }

    [Fact]
    public void IsValidName_SpacePaddedSingleChar_ReturnsFalse()
    {
        // "  A  " trimmed = "A" (length 1) → invalid
        Assert.False(PersonValidator.IsValidName("  A  "));
    }

    [Fact]
    public void IsValidName_DefaultRequiredParameter_DefaultsToTrue()
    {
        // required defaults to true
        Assert.False(PersonValidator.IsValidName(null));
    }

    [Fact]
    public void IsValidName_SpecialCharacters_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName("O'Brien"));

    [Fact]
    public void IsValidName_Numbers_Valid()
        => Assert.True(PersonValidator.IsValidName("Abc123"));

    [Fact]
    public void IsValidName_Unicode_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName("José"));

    [Fact]
    public void IsValidName_NameWithSpaces_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName("Smith Family"));

    [Fact]
    public void IsValidName_NameWithHyphen_ReturnsFalse()
        => Assert.False(PersonValidator.IsValidName("Mary-Jane"));

    [Fact]
    public void IsValidName_PureAlphanumeric_ReturnsTrue()
        => Assert.True(PersonValidator.IsValidName("SmithFamily123"));

    [Fact]
    public void Sanitize_NullInput_ReturnsEmpty()
        => Assert.Equal("", PersonValidator.Sanitize(null));

    [Fact]
    public void Sanitize_EmptyInput_ReturnsEmpty()
        => Assert.Equal("", PersonValidator.Sanitize(""));

    [Fact]
    public void Sanitize_PureAlphanumeric_ReturnsUnchanged()
        => Assert.Equal("Alice123", PersonValidator.Sanitize("Alice123"));

    [Fact]
    public void Sanitize_RemovesSpaces()
        => Assert.Equal("SmithFamily", PersonValidator.Sanitize("Smith Family"));

    [Fact]
    public void Sanitize_RemovesSpecialChars()
        => Assert.Equal("OBrien", PersonValidator.Sanitize("O'Brien!"));

    [Fact]
    public void Sanitize_RemovesUnicode()
        => Assert.Equal("Jos", PersonValidator.Sanitize("José"));

    // ── IsValidBirthDate ───────────────────────────────

    [Fact]
    public void IsValidBirthDate_Yesterday_ReturnsTrue()
    {
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        Assert.True(PersonValidator.IsValidBirthDate(yesterday));
    }

    [Fact]
    public void IsValidBirthDate_Today_ReturnsFalse()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Assert.False(PersonValidator.IsValidBirthDate(today));
    }

    [Fact]
    public void IsValidBirthDate_Tomorrow_ReturnsFalse()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        Assert.False(PersonValidator.IsValidBirthDate(tomorrow));
    }

    [Fact]
    public void IsValidBirthDate_Year1900_ReturnsTrue()
    {
        var date = new DateOnly(1900, 1, 1);
        Assert.True(PersonValidator.IsValidBirthDate(date));
    }

    [Fact]
    public void IsValidBirthDate_Year1899_ReturnsFalse()
    {
        var date = new DateOnly(1899, 12, 31);
        Assert.False(PersonValidator.IsValidBirthDate(date));
    }

    [Fact]
    public void IsValidBirthDate_VeryOldDate_ReturnsFalse()
    {
        var date = new DateOnly(1000, 6, 15);
        Assert.False(PersonValidator.IsValidBirthDate(date));
    }

    [Fact]
    public void IsValidBirthDate_RecentPastDate_ReturnsTrue()
    {
        var date = new DateOnly(2000, 6, 15);
        Assert.True(PersonValidator.IsValidBirthDate(date));
    }
}
