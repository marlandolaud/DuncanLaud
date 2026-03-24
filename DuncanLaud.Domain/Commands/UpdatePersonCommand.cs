namespace DuncanLaud.Domain.Commands;

public record UpdatePersonCommand(
    Guid PersonId,
    Guid GroupId,
    string FirstName,
    string LastName,
    string? PreferredName,
    DateOnly BirthDate,
    byte[]? ImageData,
    string? ImageContentType,
    bool RemoveImage
);
