using System.ComponentModel.DataAnnotations;

namespace DuncanLaud.DTOs;

public record CreateGroupRequest(
    [Required] Guid GroupId,
    [Required, MinLength(2), MaxLength(100)] string Name
);
