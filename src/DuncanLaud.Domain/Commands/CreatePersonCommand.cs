namespace DuncanLaud.Domain.Commands;

public record CreatePersonCommand(
    Guid GroupId,
    string FirstName,
    string LastName,
    string? PreferredName,
    DateOnly BirthDate,
    byte[]? ImageData,
    string? ImageContentType,
    string? Email
);
