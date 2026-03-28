namespace DuncanLaud.Domain.Services;

/// <summary>
/// Pure business logic for birthday calculations. No external dependencies.
/// </summary>
public static class BirthdayCalculator
{
    /// <summary>
    /// Computes the number of days from today until the next occurrence of the given birthday.
    /// Returns 0 if today is the birthday. Handles leap-year birthdays (Feb 29 → Feb 28 in non-leap years).
    /// </summary>
    public static int DaysUntil(DateOnly birthDate, DateOnly today)
    {
        var month = birthDate.Month;
        var day = birthDate.Day;

        // Feb 29 in a non-leap year: treat as Feb 28
        if (month == 2 && day == 29 && !DateTime.IsLeapYear(today.Year))
        {
            day = 28;
        }

        var thisYearBirthday = new DateOnly(today.Year, month, day);
        if (thisYearBirthday < today)
        {
            var nextYear = today.Year + 1;
            if (month == 2 && day == 29 && !DateTime.IsLeapYear(nextYear))
                day = 28;
            thisYearBirthday = new DateOnly(nextYear, month, day);
        }

        return thisYearBirthday.DayNumber - today.DayNumber;
    }

    /// <summary>
    /// Filters and sorts a collection of people by upcoming birthday within a window.
    /// </summary>
    public static IReadOnlyList<ValueObjects.BirthdayResult> GetUpcoming<T>(
        IEnumerable<T> people,
        Func<T, Guid> getId,
        Func<T, string> getDisplayName,
        Func<T, DateOnly> getBirthDate,
        Func<T, bool> getHasImage,
        int daysAhead,
        DateOnly today)
    {
        return people
            .Select(p =>
            {
                var daysUntil = DaysUntil(getBirthDate(p), today);
                return new ValueObjects.BirthdayResult(
                    getId(p),
                    getDisplayName(p),
                    getBirthDate(p),
                    daysUntil,
                    getHasImage(p));
            })
            .Where(r => r.DaysUntil <= daysAhead)
            .OrderBy(r => r.DaysUntil)
            .ToList();
    }
}
