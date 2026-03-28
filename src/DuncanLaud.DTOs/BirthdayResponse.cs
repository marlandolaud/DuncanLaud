namespace DuncanLaud.DTOs;

public record BirthdayResponse(
    Guid PersonId,
    string DisplayName,
    string BirthDateDisplay,
    int DaysUntil,
    bool HasImage
);
