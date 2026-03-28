namespace DuncanLaud.DTOs;

public record GroupResponse(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    int MemberCount
);
