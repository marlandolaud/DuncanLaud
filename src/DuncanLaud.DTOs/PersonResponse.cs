namespace DuncanLaud.DTOs;

public record PersonResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? PreferredName,
    DateOnly BirthDate,
    string? Email,
    bool HasImage,
    DateTime CreatedAtUtc
);
