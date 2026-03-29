using DuncanLaud.Domain.Commands;
using DuncanLaud.DTOs;
using System.ComponentModel.DataAnnotations;
using DuncanLaud.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DuncanLaud.WebUI.Controllers;

[ApiController]
[Route("api/group")]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IPersonService _personService;
    private readonly IPersonRepository _personRepo;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB

    public GroupController(IGroupService groupService, IPersonService personService, IPersonRepository personRepo)
    {
        _groupService = groupService;
        _personService = personService;
        _personRepo = personRepo;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest dto, CancellationToken ct)
    {
        try
        {
            var group = await _groupService.GetOrCreateGroupAsync(dto.GroupId, dto.Name, ct);

            var response = new GroupResponse(group.Id, group.Name, group.CreatedAtUtc, group.Members.Count);
            return CreatedAtAction(nameof(GetGroup), new { groupId = group.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{groupId:guid}")]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroup(Guid groupId, CancellationToken ct)
    {
        var group = await _groupService.GetGroupAsync(groupId, ct);
        if (group is null)
            return NotFound();

        return Ok(new GroupResponse(group.Id, group.Name, group.CreatedAtUtc, group.Members.Count));
    }

    [HttpPost("{groupId:guid}/person")]
    [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddPerson(
        Guid groupId,
        [FromForm] string firstName,
        [FromForm] string lastName,
        [FromForm] string? preferredName,
        [FromForm] DateOnly birthDate,
        [FromForm] string? email,
        IFormFile? image,
        CancellationToken ct)
    {
        var group = await _groupService.GetGroupAsync(groupId, ct);
        if (group is null)
            return NotFound();

        byte[]? imageData = null;
        string? imageContentType = null;

        if (image is not null && image.Length > 0)
        {
            if (image.Length > MaxImageSize)
                return BadRequest(new { error = "Image must be 5 MB or smaller." });

            if (!AllowedContentTypes.Contains(image.ContentType))
                return BadRequest(new { error = "Image must be JPEG, PNG, WebP, or GIF." });

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, ct);
            imageData = ms.ToArray();
            imageContentType = image.ContentType;
        }

        try
        {
            var command = new CreatePersonCommand(
                groupId,
                firstName,
                lastName,
                preferredName,
                birthDate,
                imageData,
                imageContentType,
                email);

            var person = await _personService.AddPersonAsync(command, ct);

            var response = new PersonResponse(
                person.Id,
                person.FirstName,
                person.LastName,
                person.PreferredName,
                person.BirthDate,
                person.Email,
                person.ImageData is not null,
                person.CreatedAtUtc);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{groupId:guid}/persons")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersons(Guid groupId, CancellationToken ct)
    {
        var group = await _groupService.GetGroupAsync(groupId, ct);
        if (group is null)
            return NotFound();

        var persons = await _personService.GetAllByGroupAsync(groupId, ct);

        var response = persons.Select(p => new PersonResponse(
            p.Id,
            p.FirstName,
            p.LastName,
            p.PreferredName,
            p.BirthDate,
            p.Email,
            p.ImageData is not null,
            p.CreatedAtUtc));

        return Ok(response);
    }

    [HttpGet("{groupId:guid}/person/{personId:guid}")]
    [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPerson(Guid groupId, Guid personId, CancellationToken ct)
    {
        var person = await _personRepo.GetByIdAsync(personId, ct);
        if (person is null || person.GroupId != groupId)
            return NotFound();

        return Ok(new PersonResponse(
            person.Id,
            person.FirstName,
            person.LastName,
            person.PreferredName,
            person.BirthDate,
            person.Email,
            person.ImageData is not null,
            person.CreatedAtUtc));
    }

    [HttpPut("{groupId:guid}/person/{personId:guid}")]
    [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdatePerson(
        Guid groupId,
        Guid personId,
        [FromForm] string firstName,
        [FromForm] string lastName,
        [FromForm] string? preferredName,
        [FromForm] DateOnly birthDate,
        [FromForm] bool removeImage,
        [FromForm] string? email,
        IFormFile? image,
        CancellationToken ct)
    {
        var group = await _groupService.GetGroupAsync(groupId, ct);
        if (group is null)
            return NotFound();

        byte[]? imageData = null;
        string? imageContentType = null;

        if (image is not null && image.Length > 0)
        {
            if (image.Length > MaxImageSize)
                return BadRequest(new { error = "Image must be 5 MB or smaller." });

            if (!AllowedContentTypes.Contains(image.ContentType))
                return BadRequest(new { error = "Image must be JPEG, PNG, WebP, or GIF." });

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, ct);
            imageData = ms.ToArray();
            imageContentType = image.ContentType;
        }

        try
        {
            var command = new UpdatePersonCommand(
                personId,
                groupId,
                firstName,
                lastName,
                preferredName,
                birthDate,
                imageData,
                imageContentType,
                removeImage,
                email);

            var person = await _personService.UpdatePersonAsync(command, ct);

            return Ok(new PersonResponse(
                person.Id,
                person.FirstName,
                person.LastName,
                person.PreferredName,
                person.BirthDate,
                person.Email,
                person.ImageData is not null,
                person.CreatedAtUtc));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{groupId:guid}/person/{personId:guid}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonImage(Guid groupId, Guid personId, CancellationToken ct)
    {
        var person = await _personRepo.GetByIdAsync(personId, ct);
        if (person is null || person.GroupId != groupId || person.ImageData is null)
            return NotFound();

        return File(person.ImageData, person.ImageContentType ?? "image/jpeg");
    }

    [HttpDelete("{groupId:guid}/person/{personId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePerson(Guid groupId, Guid personId, CancellationToken ct)
    {
        var group = await _groupService.GetGroupAsync(groupId, ct);
        if (group is null)
            return NotFound();

        try
        {
            await _personService.DeletePersonAsync(groupId, personId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{groupId:guid}/name")]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGroupName(Guid groupId, [FromBody] UpdateGroupNameRequest dto, CancellationToken ct)
    {
        try
        {
            var group = await _groupService.UpdateGroupNameAsync(groupId, dto.Name, ct);
            return Ok(new GroupResponse(group.Id, group.Name, group.CreatedAtUtc, group.Members.Count));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{groupId:guid}/birthdays")]
    [ProducesResponseType(typeof(IReadOnlyList<BirthdayResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBirthdays(Guid groupId, CancellationToken ct)
    {
        var group = await _groupService.GetGroupAsync(groupId, ct);
        if (group is null)
            return NotFound();

        var results = await _personService.GetUpcomingBirthdaysAsync(groupId, ct);

        var response = results.Select(r => new BirthdayResponse(
            r.PersonId,
            r.DisplayName,
            r.BirthDate.ToString("MMMM d"),
            r.DaysUntil,
            r.HasImage));

        return Ok(response);
    }
}
