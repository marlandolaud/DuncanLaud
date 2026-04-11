using DuncanLaud.Domain.Services;
using DuncanLaud.Domain.ValueObjects;

namespace DuncanLaud.Domain.Tests;

public class BirthdayCalculatorTests
{
    // ── DaysUntil ──────────────────────────────────────

    [Fact]
    public void DaysUntil_BirthdayIsToday_ReturnsZero()
    {
        var today = new DateOnly(2026, 3, 22);
        var birth = new DateOnly(1990, 3, 22);

        Assert.Equal(0, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_BirthdayIsTomorrow_ReturnsOne()
    {
        var today = new DateOnly(2026, 3, 22);
        var birth = new DateOnly(1990, 3, 23);
        Assert.Equal(1, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_BirthdayWasYesterday_Returns364Or365()
    {
        var today = new DateOnly(2026, 3, 22);
        var birth = new DateOnly(1990, 3, 21);
        Assert.Equal(364, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_BirthdayLaterThisYear_ReturnsCorrectDays()
    {
        var today = new DateOnly(2026, 1, 1);
        var birth = new DateOnly(1990, 6, 15);
        Assert.Equal(165, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_BirthdayEarlierThisYear_WrapsToNextYear()
    {
        var today = new DateOnly(2026, 6, 15);
        var birth = new DateOnly(1990, 1, 1);
        Assert.Equal(200, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_LeapYearBirthday_InLeapYear_ReturnsFeb29()
    {
        var today = new DateOnly(2028, 2, 1);
        var birth = new DateOnly(2000, 2, 29);
        Assert.Equal(28, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_LeapYearBirthday_InNonLeapYear_UsesFeb28()
    {
        var today = new DateOnly(2026, 2, 1);
        var birth = new DateOnly(2000, 2, 29);
        Assert.Equal(27, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_LeapYearBirthday_AfterFeb28InNonLeapYear_WrapsToNextYear()
    {
        var today = new DateOnly(2027, 3, 1);
        var birth = new DateOnly(2000, 2, 29);
        Assert.Equal(364, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_LeapYearBirthday_TodayIsFeb28InNonLeapYear_ReturnsZero()
    {
        var today = new DateOnly(2026, 2, 28);
        var birth = new DateOnly(2000, 2, 29);
        Assert.Equal(0, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_LeapYearBirthday_NextYearAlsoNotLeap_UsesFeb28()
    {
        var today = new DateOnly(2025, 3, 1);
        var birth = new DateOnly(2000, 2, 29);
        Assert.Equal(364, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_LeapYearBirthday_PassedInLeapYear_NextYearNotLeap_UsesFeb28()
    {
        // Today is March 1, 2028 (leap year). Feb 29, 2028 already passed.
        // In leap year, day stays 29. thisYearBirthday = Feb 29 < Mar 1 → wraps.
        // Next year 2029 is non-leap, so line 28 fires: day becomes 28.
        var today = new DateOnly(2028, 3, 1);
        var birth = new DateOnly(2000, 2, 29);
        var expected = new DateOnly(2029, 2, 28).DayNumber - today.DayNumber;
        Assert.Equal(expected, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_Dec31Birthday_OnJan1_Returns364()
    {
        var today = new DateOnly(2026, 1, 1);
        var birth = new DateOnly(1990, 12, 31);
        Assert.Equal(364, BirthdayCalculator.DaysUntil(birth, today));
    }

    [Fact]
    public void DaysUntil_Jan1Birthday_OnDec31_ReturnsOne()
    {
        var today = new DateOnly(2026, 12, 31);
        var birth = new DateOnly(1990, 1, 1);
        Assert.Equal(1, BirthdayCalculator.DaysUntil(birth, today));
    }

    // ── GetUpcoming ────────────────────────────────────

    [Fact]
    public void GetUpcoming_EmptyCollection_ReturnsEmptyList()
    {
        var people = Array.Empty<(Guid Id, string Name, DateOnly Birth, bool HasImg)>();
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            60, new DateOnly(2026, 3, 22));
        Assert.Empty(result);
    }

    [Fact]
    public void GetUpcoming_PersonWithinWindow_ReturnsIt()
    {
        var id = Guid.NewGuid();
        var people = new[] { (Id: id, Name: "Alice", Birth: new DateOnly(1990, 4, 1), HasImg: true) };
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            60, new DateOnly(2026, 3, 22));
        Assert.Single(result);
        Assert.Equal("Alice", result[0].DisplayName);
        Assert.Equal(10, result[0].DaysUntil);
    }

    [Fact]
    public void GetUpcoming_PersonOutsideWindow_Excluded()
    {
        var people = new[] { (Id: Guid.NewGuid(), Name: "Alice", Birth: new DateOnly(1990, 9, 1), HasImg: false) };
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            60, new DateOnly(2026, 3, 22));
        Assert.Empty(result);
    }

    [Fact]
    public void GetUpcoming_MultiplePeople_SortedByDaysUntil()
    {
        var people = new[]
        {
            (Id: Guid.NewGuid(), Name: "Far", Birth: new DateOnly(1990, 5, 1), HasImg: false),
            (Id: Guid.NewGuid(), Name: "Near", Birth: new DateOnly(1990, 3, 25), HasImg: false),
            (Id: Guid.NewGuid(), Name: "Mid", Birth: new DateOnly(1990, 4, 10), HasImg: false),
        };
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            60, new DateOnly(2026, 3, 22));
        Assert.Equal(3, result.Count);
        Assert.Equal("Near", result[0].DisplayName);
        Assert.Equal("Mid", result[1].DisplayName);
        Assert.Equal("Far", result[2].DisplayName);
    }

    [Fact]
    public void GetUpcoming_DaysAheadZero_OnlyTodayBirthdays()
    {
        var people = new[]
        {
            (Id: Guid.NewGuid(), Name: "Today", Birth: new DateOnly(1990, 3, 22), HasImg: false),
            (Id: Guid.NewGuid(), Name: "Tomorrow", Birth: new DateOnly(1990, 3, 23), HasImg: false),
        };
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            0, new DateOnly(2026, 3, 22));
        Assert.Single(result);
        Assert.Equal("Today", result[0].DisplayName);
        Assert.Equal(0, result[0].DaysUntil);
    }

    [Fact]
    public void GetUpcoming_MixedInsideAndOutsideWindow_FiltersCorrectly()
    {
        var people = new[]
        {
            (Id: Guid.NewGuid(), Name: "In", Birth: new DateOnly(1990, 4, 1), HasImg: false),
            (Id: Guid.NewGuid(), Name: "Out", Birth: new DateOnly(1990, 12, 25), HasImg: false),
        };
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            60, new DateOnly(2026, 3, 22));
        Assert.Single(result);
        Assert.Equal("In", result[0].DisplayName);
    }

    [Fact]
    public void GetUpcoming_HasImageTrue_PreservedInResult()
    {
        var people = new[] { (Id: Guid.NewGuid(), Name: "Alice", Birth: new DateOnly(1990, 3, 25), HasImg: true) };
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            60, new DateOnly(2026, 3, 22));
        Assert.True(result[0].HasImage);
    }

    [Fact]
    public void GetUpcoming_HasImageFalse_PreservedInResult()
    {
        var people = new[] { (Id: Guid.NewGuid(), Name: "Alice", Birth: new DateOnly(1990, 3, 25), HasImg: false) };
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            60, new DateOnly(2026, 3, 22));
        Assert.False(result[0].HasImage);
    }

    [Fact]
    public void GetUpcoming_PreservesPersonIdAndBirthDate()
    {
        var id = Guid.NewGuid();
        var birth = new DateOnly(1990, 3, 25);
        var people = new[] { (Id: id, Name: "Alice", Birth: birth, HasImg: false) };
        var result = BirthdayCalculator.GetUpcoming(
            people, p => p.Id, p => p.Name, p => p.Birth, p => p.HasImg,
            60, new DateOnly(2026, 3, 22));
        Assert.Equal(id, result[0].PersonId);
        Assert.Equal(birth, result[0].BirthDate);
    }
}
