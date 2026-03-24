namespace DuncanLaud.Domain.ValueObjects;

public record BirthdayResult(
    Guid PersonId,
    string DisplayName,
    DateOnly BirthDate,
    int DaysUntil,
    bool HasImage
);
