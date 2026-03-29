using System.ComponentModel.DataAnnotations;

namespace DuncanLaud.DTOs;

public record UpdateGroupNameRequest([Required] string Name);
