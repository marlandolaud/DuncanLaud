using System.ComponentModel.DataAnnotations;

namespace DuncanLaud.DTOs;

public record CreatePersonRequest(
    [Required, MinLength(2), MaxLength(100)] string FirstName,
    [Required, MinLength(2), MaxLength(100)] string LastName,
    [MinLength(2), MaxLength(100)] string? PreferredName,
    [Required] DateOnly BirthDate
);
