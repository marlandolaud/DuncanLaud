namespace DuncanLaud.Infrastructure.Entities;

public class Person
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PreferredName { get; set; }
    public DateOnly BirthDate { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageContentType { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Group Group { get; set; } = null!;
}
