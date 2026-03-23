using DuncanLaud.Domain.Commands;
using DuncanLaud.Domain.ValueObjects;
using DuncanLaud.DTOs;
using DuncanLaud.Infrastructure.Entities;
using DuncanLaud.Infrastructure.Interfaces;
using DuncanLaud.WebUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DuncanLaud.WebUI.Tests;

public class GroupControllerTests
{
    private readonly Mock<IGroupService> _groupServiceMock = new();
    private readonly Mock<IPersonService> _personServiceMock = new();
    private readonly Mock<IPersonRepository> _personRepoMock = new();
    private readonly GroupController _sut;

    public GroupControllerTests() =>
        _sut = new GroupController(_groupServiceMock.Object, _personServiceMock.Object, _personRepoMock.Object);

    // ── CreateGroup ────────────────────────────────────

    [Fact]
    public async Task CreateGroup_ValidRequest_Returns201Created()
    {
        var groupId = Guid.NewGuid();
        var group = new Group { Id = groupId, Name = "Family", CreatedAtUtc = DateTime.UtcNow, Members = new List<Person>() };
        _groupServiceMock.Setup(s => s.GetOrCreateGroupAsync(groupId, "Family", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(group);

        var result = await _sut.CreateGroup(new CreateGroupRequest(groupId, "Family"), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        var response = Assert.IsType<GroupResponse>(created.Value);
        Assert.Equal(groupId, response.Id);
        Assert.Equal("Family", response.Name);
        Assert.Equal(0, response.MemberCount);
    }

    // ── GetGroup ───────────────────────────────────────

    [Fact]
    public async Task GetGroup_Exists_Returns200()
    {
        var id = Guid.NewGuid();
        var group = new Group { Id = id, Name = "Test", CreatedAtUtc = DateTime.UtcNow, Members = new List<Person>() };
        _groupServiceMock.Setup(s => s.GetGroupAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(group);

        var result = await _sut.GetGroup(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GroupResponse>(ok.Value);
        Assert.Equal("Test", response.Name);
    }

    [Fact]
    public async Task GetGroup_NotFound_Returns404()
    {
        _groupServiceMock.Setup(s => s.GetGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Group?)null);

        var result = await _sut.GetGroup(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetGroup_WithMembers_ReturnsCorrectCount()
    {
        var id = Guid.NewGuid();
        var group = new Group
        {
            Id = id, Name = "G", CreatedAtUtc = DateTime.UtcNow,
            Members = new List<Person>
            {
                new() { Id = Guid.NewGuid(), GroupId = id, FirstName = "A", LastName = "B",
                         BirthDate = new DateOnly(2000,1,1), CreatedAtUtc = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), GroupId = id, FirstName = "C", LastName = "D",
                         BirthDate = new DateOnly(2001,2,2), CreatedAtUtc = DateTime.UtcNow },
            }
        };
        _groupServiceMock.Setup(s => s.GetGroupAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(group);

        var result = await _sut.GetGroup(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GroupResponse>(ok.Value);
        Assert.Equal(2, response.MemberCount);
    }

    // ── AddPerson ──────────────────────────────────────

    [Fact]
    public async Task AddPerson_GroupNotFound_Returns404()
    {
        _groupServiceMock.Setup(s => s.GetGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Group?)null);

        var result = await _sut.AddPerson(Guid.NewGuid(), "Alice", "Smith", null, new DateOnly(2000, 1, 1), null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddPerson_ValidRequest_Returns201()
    {
        var groupId = Guid.NewGuid();
        var group = new Group { Id = groupId, Name = "G", Members = new List<Person>() };
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync(group);

        var person = new Person
        {
            Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _personServiceMock.Setup(s => s.AddPersonAsync(It.IsAny<CreatePersonCommand>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(person);

        var result = await _sut.AddPerson(groupId, "Alice", "Smith", null, new DateOnly(2000, 1, 1), null, CancellationToken.None);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);
        var response = Assert.IsType<PersonResponse>(obj.Value);
        Assert.Equal("Alice", response.FirstName);
        Assert.False(response.HasImage);
    }

    [Fact]
    public async Task AddPerson_WithImage_Returns201WithHasImageTrue()
    {
        var groupId = Guid.NewGuid();
        var group = new Group { Id = groupId, Name = "G", Members = new List<Person>() };
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync(group);

        var person = new Person
        {
            Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = new byte[] { 1, 2, 3 }, ImageContentType = "image/jpeg"
        };
        _personServiceMock.Setup(s => s.AddPersonAsync(It.IsAny<CreatePersonCommand>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(person);

        var imageFile = CreateMockFormFile("test.jpg", "image/jpeg", new byte[] { 1, 2, 3 });
        var result = await _sut.AddPerson(groupId, "Alice", "Smith", null, new DateOnly(2000, 1, 1), imageFile, CancellationToken.None);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);
        var response = Assert.IsType<PersonResponse>(obj.Value);
        Assert.True(response.HasImage);
    }

    [Fact]
    public async Task AddPerson_ImageTooLarge_Returns400()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });

        var largeImage = CreateMockFormFile("big.jpg", "image/jpeg", new byte[6 * 1024 * 1024]);
        var result = await _sut.AddPerson(groupId, "Alice", "Smith", null, new DateOnly(2000, 1, 1), largeImage, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddPerson_InvalidContentType_Returns400()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });

        var badFile = CreateMockFormFile("hack.exe", "application/octet-stream", new byte[] { 1 });
        var result = await _sut.AddPerson(groupId, "Alice", "Smith", null, new DateOnly(2000, 1, 1), badFile, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddPerson_ServiceThrowsArgumentException_Returns400()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });
        _personServiceMock.Setup(s => s.AddPersonAsync(It.IsAny<CreatePersonCommand>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new ArgumentException("First name contains inappropriate content."));

        var result = await _sut.AddPerson(groupId, "BadWord", "Smith", null, new DateOnly(2000, 1, 1), null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddPerson_NullImage_Accepted()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });
        _personServiceMock.Setup(s => s.AddPersonAsync(It.IsAny<CreatePersonCommand>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new Person
                          {
                              Id = Guid.NewGuid(), GroupId = groupId, FirstName = "A", LastName = "B",
                              BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
                          });

        var result = await _sut.AddPerson(groupId, "Al", "Sm", null, new DateOnly(2000, 1, 1), null, CancellationToken.None);

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, obj.StatusCode);
    }

    // ── GetPersonImage ───────────────────────────────

    [Fact]
    public async Task GetPersonImage_PersonNotFound_Returns404()
    {
        _personRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Person?)null);

        var result = await _sut.GetPersonImage(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPersonImage_WrongGroup_Returns404()
    {
        var personId = Guid.NewGuid();
        var person = new Person
        {
            Id = personId, GroupId = Guid.NewGuid(), FirstName = "A", LastName = "B",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = new byte[] { 1 }, ImageContentType = "image/png"
        };
        _personRepoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(person);

        var result = await _sut.GetPersonImage(Guid.NewGuid(), personId, CancellationToken.None); // wrong groupId

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPersonImage_NoImageData_Returns404()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var person = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "A", LastName = "B",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = null
        };
        _personRepoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(person);

        var result = await _sut.GetPersonImage(groupId, personId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPersonImage_HasImage_ReturnsFileResult()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var imgData = new byte[] { 0xFF, 0xD8, 0xFF };
        var person = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "A", LastName = "B",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = imgData, ImageContentType = "image/jpeg"
        };
        _personRepoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(person);

        var result = await _sut.GetPersonImage(groupId, personId, CancellationToken.None);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("image/jpeg", fileResult.ContentType);
        Assert.Equal(imgData, fileResult.FileContents);
    }

    [Fact]
    public async Task GetPersonImage_NullContentType_DefaultsToJpeg()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var person = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "A", LastName = "B",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = new byte[] { 1 }, ImageContentType = null
        };
        _personRepoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(person);

        var result = await _sut.GetPersonImage(groupId, personId, CancellationToken.None);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("image/jpeg", fileResult.ContentType);
    }

    // ── GetPersons ───────────────────────────────────

    [Fact]
    public async Task GetPersons_GroupNotFound_Returns404()
    {
        _groupServiceMock.Setup(s => s.GetGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Group?)null);

        var result = await _sut.GetPersons(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPersons_GroupExists_ReturnsAllMembers()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });
        _personServiceMock.Setup(s => s.GetAllByGroupAsync(groupId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new List<Person>
                          {
                              new() { Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Alice", LastName = "Smith",
                                       BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow },
                              new() { Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Bob", LastName = "Jones",
                                       BirthDate = new DateOnly(1995, 6, 15), CreatedAtUtc = DateTime.UtcNow,
                                       ImageData = new byte[] { 1 }, ImageContentType = "image/png" },
                          });

        var result = await _sut.GetPersons(groupId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<PersonResponse>>(ok.Value);
        var list = items.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("Alice", list[0].FirstName);
        Assert.False(list[0].HasImage);
        Assert.True(list[1].HasImage);
    }

    // ── GetPerson ────────────────────────────────────

    [Fact]
    public async Task GetPerson_Found_Returns200()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var person = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            PreferredName = "Ali", BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _personRepoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(person);

        var result = await _sut.GetPerson(groupId, personId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PersonResponse>(ok.Value);
        Assert.Equal("Alice", response.FirstName);
        Assert.Equal("Ali", response.PreferredName);
    }

    [Fact]
    public async Task GetPerson_NotFound_Returns404()
    {
        _personRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Person?)null);

        var result = await _sut.GetPerson(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPerson_WrongGroup_Returns404()
    {
        var personId = Guid.NewGuid();
        var person = new Person
        {
            Id = personId, GroupId = Guid.NewGuid(), FirstName = "A", LastName = "B",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _personRepoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(person);

        var result = await _sut.GetPerson(Guid.NewGuid(), personId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── UpdatePerson ─────────────────────────────────

    [Fact]
    public async Task UpdatePerson_GroupNotFound_Returns404()
    {
        _groupServiceMock.Setup(s => s.GetGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Group?)null);

        var result = await _sut.UpdatePerson(
            Guid.NewGuid(), Guid.NewGuid(), "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), false, null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdatePerson_Valid_Returns200()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });

        var updatedPerson = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Bob", LastName = "Jones",
            BirthDate = new DateOnly(1995, 6, 15), CreatedAtUtc = DateTime.UtcNow
        };
        _personServiceMock.Setup(s => s.UpdatePersonAsync(It.IsAny<UpdatePersonCommand>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(updatedPerson);

        var result = await _sut.UpdatePerson(
            groupId, personId, "Bob", "Jones", null,
            new DateOnly(1995, 6, 15), false, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PersonResponse>(ok.Value);
        Assert.Equal("Bob", response.FirstName);
        Assert.Equal("Jones", response.LastName);
    }

    [Fact]
    public async Task UpdatePerson_PersonNotFound_Returns404()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });
        _personServiceMock.Setup(s => s.UpdatePersonAsync(It.IsAny<UpdatePersonCommand>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new KeyNotFoundException("Person not found."));

        var result = await _sut.UpdatePerson(
            groupId, Guid.NewGuid(), "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), false, null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdatePerson_ValidationError_Returns400()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });
        _personServiceMock.Setup(s => s.UpdatePersonAsync(It.IsAny<UpdatePersonCommand>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new ArgumentException("First name must be between 2 and 100 characters."));

        var result = await _sut.UpdatePerson(
            groupId, Guid.NewGuid(), "A", "Smith", null,
            new DateOnly(2000, 1, 1), false, null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePerson_ImageTooLarge_Returns400()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });

        var largeImage = CreateMockFormFile("big.jpg", "image/jpeg", new byte[6 * 1024 * 1024]);
        var result = await _sut.UpdatePerson(
            groupId, Guid.NewGuid(), "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), false, largeImage, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePerson_InvalidImageType_Returns400()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });

        var badFile = CreateMockFormFile("hack.exe", "application/octet-stream", new byte[] { 1 });
        var result = await _sut.UpdatePerson(
            groupId, Guid.NewGuid(), "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), false, badFile, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePerson_WithNewImage_PassesImageToCommand()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });

        var updatedPerson = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = new byte[] { 9, 8, 7 }, ImageContentType = "image/png"
        };
        _personServiceMock.Setup(s => s.UpdatePersonAsync(It.IsAny<UpdatePersonCommand>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(updatedPerson);

        var imageFile = CreateMockFormFile("new.png", "image/png", new byte[] { 9, 8, 7 });
        var result = await _sut.UpdatePerson(
            groupId, personId, "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), false, imageFile, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PersonResponse>(ok.Value);
        Assert.True(response.HasImage);
    }

    // ── GetBirthdays ───────────────────────────────────

    [Fact]
    public async Task GetBirthdays_GroupNotFound_Returns404()
    {
        _groupServiceMock.Setup(s => s.GetGroupAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Group?)null);

        var result = await _sut.GetBirthdays(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetBirthdays_GroupExists_Returns200WithBirthdays()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });
        _personServiceMock.Setup(s => s.GetUpcomingBirthdaysAsync(groupId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new List<BirthdayResult>
                          {
                              new(Guid.NewGuid(), "Alice Smith", new DateOnly(1990, 4, 1), 10, true),
                              new(Guid.NewGuid(), "Bob Jones", new DateOnly(1985, 4, 15), 24, false),
                          });

        var result = await _sut.GetBirthdays(groupId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<BirthdayResponse>>(ok.Value);
        var list = items.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("Alice Smith", list[0].DisplayName);
        Assert.Equal("April 1", list[0].BirthDateDisplay);
        Assert.Equal(10, list[0].DaysUntil);
        Assert.True(list[0].HasImage);
        Assert.False(list[1].HasImage);
    }

    [Fact]
    public async Task GetBirthdays_EmptyList_Returns200WithEmptyArray()
    {
        var groupId = Guid.NewGuid();
        _groupServiceMock.Setup(s => s.GetGroupAsync(groupId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new Group { Id = groupId, Name = "G", Members = new List<Person>() });
        _personServiceMock.Setup(s => s.GetUpcomingBirthdaysAsync(groupId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new List<BirthdayResult>());

        var result = await _sut.GetBirthdays(groupId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<BirthdayResponse>>(ok.Value);
        Assert.Empty(items);
    }

    // ── Helper ──────────────────────────────────────────

    private static IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "image", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
