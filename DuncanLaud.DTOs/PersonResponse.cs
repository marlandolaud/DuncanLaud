namespace DuncanLaud.DTOs;

public record PersonResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? PreferredName,
    DateOnly BirthDate,
    bool HasImage,
    DateTime CreatedAtUtc
);
