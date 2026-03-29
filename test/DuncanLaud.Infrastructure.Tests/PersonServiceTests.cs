using DuncanLaud.Domain.Commands;
using DuncanLaud.Infrastructure.Entities;
using DuncanLaud.Infrastructure.Interfaces;
using DuncanLaud.Infrastructure.Services;
using Moq;

namespace DuncanLaud.Infrastructure.Tests;

public class PersonServiceTests
{
    private readonly Mock<IPersonRepository> _repoMock = new();
    private readonly PersonService _sut;

    public PersonServiceTests() => _sut = new PersonService(_repoMock.Object);

    private static CreatePersonCommand ValidCommand(
        string firstName = "Alice",
        string lastName = "Smith",
        string? preferredName = null,
        DateOnly? birthDate = null,
        byte[]? imageData = null,
        string? imageContentType = null,
        string? email = null)
    {
        return new CreatePersonCommand(
            Guid.NewGuid(),
            firstName,
            lastName,
            preferredName,
            birthDate ?? new DateOnly(2000, 1, 15),
            imageData,
            imageContentType,
            email);
    }

    // ── AddPersonAsync — success ───────────────────────

    [Fact]
    public async Task AddPersonAsync_ValidCommand_CreatesPerson()
    {
        var cmd = ValidCommand();
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPersonAsync_SanitizesNames()
    {
        // "  Bob  " sanitizes to "Bob", "  Jones  " to "Jones"
        var cmd = ValidCommand(firstName: "  Bob  ", lastName: "  Jones  ");
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Equal("Bob", result.FirstName);
        Assert.Equal("Jones", result.LastName);
    }

    [Fact]
    public async Task AddPersonAsync_SanitizesPreferredName()
    {
        // "  Bobby  " sanitizes to "Bobby"
        var cmd = ValidCommand(preferredName: "  Bobby  ");
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Equal("Bobby", result.PreferredName);
    }

    [Fact]
    public async Task AddPersonAsync_NullPreferredName_AllowedAsNull()
    {
        var cmd = ValidCommand(preferredName: null);
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Null(result.PreferredName);
    }

    [Fact]
    public async Task AddPersonAsync_SetsImageData()
    {
        var imgData = new byte[] { 1, 2, 3 };
        var cmd = ValidCommand(imageData: imgData, imageContentType: "image/png");
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Equal(imgData, result.ImageData);
        Assert.Equal("image/png", result.ImageContentType);
    }

    [Fact]
    public async Task AddPersonAsync_NullImage_AllowedAsNull()
    {
        var cmd = ValidCommand();
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Null(result.ImageData);
        Assert.Null(result.ImageContentType);
    }

    [Fact]
    public async Task AddPersonAsync_SetsCreatedAtUtcAndId()
    {
        var before = DateTime.UtcNow;
        var result = await _sut.AddPersonAsync(ValidCommand(), CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.InRange(result.CreatedAtUtc, before, after);
    }

    // ── AddPersonAsync — validation failures ───────────

    [Theory]
    [InlineData("A")]       // too short
    [InlineData("")]        // empty
    [InlineData("   ")]     // whitespace
    public async Task AddPersonAsync_InvalidFirstName_Throws(string firstName)
    {
        var cmd = ValidCommand(firstName: firstName);
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("First name", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_FirstNameTooLong_Throws()
    {
        var cmd = ValidCommand(firstName: new string('A', 101));
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("First name", ex.Message);
    }

    [Theory]
    [InlineData("B")]
    [InlineData("")]
    public async Task AddPersonAsync_InvalidLastName_Throws(string lastName)
    {
        var cmd = ValidCommand(lastName: lastName);
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("Last name", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_LastNameTooLong_Throws()
    {
        var cmd = ValidCommand(lastName: new string('B', 101));
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("Last name", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_PreferredNameTooShort_Throws()
    {
        var cmd = ValidCommand(preferredName: "X");
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("Preferred name", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_BirthDateToday_Throws()
    {
        var cmd = ValidCommand(birthDate: DateOnly.FromDateTime(DateTime.UtcNow));
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("Birth date", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_BirthDateFuture_Throws()
    {
        var cmd = ValidCommand(birthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("Birth date", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_BirthDateBefore1900_Throws()
    {
        var cmd = ValidCommand(birthDate: new DateOnly(1899, 12, 31));
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("Birth date", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_SpecialCharsOnlyFirstName_Throws()
    {
        // "!@#$" sanitized to "" → throws "First name"
        var cmd = ValidCommand(firstName: "!@#$");
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("First name", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_NamesWithEmbeddedSpecialChars_SanitizesBeforeStore()
    {
        // "Al!ice" → "Alice", "Sm@ith" → "Smith"
        var cmd = ValidCommand(firstName: "Al!ice", lastName: "Sm@ith");
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("Smith", result.LastName);
    }

    // ── AddPersonAsync — profanity ────────────────────

    [Fact]
    public async Task AddPersonAsync_ProfaneFirstName_Throws()
    {
        // "shit" passes Sanitize (alphanumeric) but triggers CheckProfanity → line 139 covered
        var cmd = ValidCommand(firstName: "shit");
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("inappropriate", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_ProfaneLastName_Throws()
    {
        var cmd = ValidCommand(lastName: "shit");
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("inappropriate", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_ProfanePreferredName_Throws()
    {
        var cmd = ValidCommand(preferredName: "shit");
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("inappropriate", ex.Message);
    }

    // ── AddPersonAsync — email ─────────────────────────

    [Fact]
    public async Task AddPersonAsync_ValidEmail_StoresEmail()
    {
        var cmd = ValidCommand(email: "alice@example.com");
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public async Task AddPersonAsync_NullEmail_StoresNull()
    {
        var cmd = ValidCommand(email: null);
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Null(result.Email);
    }

    [Fact]
    public async Task AddPersonAsync_EmptyEmail_StoresNull()
    {
        var cmd = ValidCommand(email: "  ");
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Null(result.Email);
    }

    [Fact]
    public async Task AddPersonAsync_EmailWithWhitespace_Trimmed()
    {
        var cmd = ValidCommand(email: "  alice@example.com  ");
        var result = await _sut.AddPersonAsync(cmd, CancellationToken.None);

        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public async Task AddPersonAsync_InvalidEmail_Throws()
    {
        var cmd = ValidCommand(email: "not-an-email");
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("Email", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_EmailTooLong_Throws()
    {
        var longEmail = new string('a', 246) + "@test.com"; // 255 chars total, > 254 limit
        var cmd = ValidCommand(email: longEmail);
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("254", ex.Message);
    }

    [Fact]
    public async Task AddPersonAsync_EmailWithDisplayName_Throws()
    {
        // MailAddress parses "Display <addr>" but addr.Address != original input → line 155 covered
        var cmd = ValidCommand(email: "\"Alice Smith\" <alice@example.com>");
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddPersonAsync(cmd, CancellationToken.None));
        Assert.Contains("Email", ex.Message);
    }

    // ── UpdatePersonAsync ──────────────────────────────

    [Fact]
    public async Task UpdatePersonAsync_Valid_UpdatesFields()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var cmd = new UpdatePersonCommand(personId, groupId, "Bob", "Jones", "Bobby",
            new DateOnly(1995, 6, 15), null, null, false, null);

        var result = await _sut.UpdatePersonAsync(cmd, CancellationToken.None);

        Assert.Equal("Bob", result.FirstName);
        Assert.Equal("Jones", result.LastName);
        Assert.Equal("Bobby", result.PreferredName);
        Assert.Equal(new DateOnly(1995, 6, 15), result.BirthDate);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePersonAsync_PersonNotFound_ThrowsKeyNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Person?)null);

        var cmd = new UpdatePersonCommand(Guid.NewGuid(), Guid.NewGuid(), "Al", "Sm", null,
            new DateOnly(2000, 1, 1), null, null, false, null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdatePersonAsync(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task UpdatePersonAsync_WrongGroup_ThrowsKeyNotFound()
    {
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = Guid.NewGuid(), FirstName = "A", LastName = "B",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var cmd = new UpdatePersonCommand(personId, Guid.NewGuid(), "Al", "Sm", null,
            new DateOnly(2000, 1, 1), null, null, false, null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdatePersonAsync(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task UpdatePersonAsync_InvalidName_ThrowsArgument()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var cmd = new UpdatePersonCommand(personId, groupId, "A", "Smith", null,
            new DateOnly(2000, 1, 1), null, null, false, null);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdatePersonAsync(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task UpdatePersonAsync_WithNewImage_ReplacesImage()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = new byte[] { 1, 2 }, ImageContentType = "image/jpeg"
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var newImg = new byte[] { 9, 8, 7 };
        var cmd = new UpdatePersonCommand(personId, groupId, "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), newImg, "image/png", false, null);

        var result = await _sut.UpdatePersonAsync(cmd, CancellationToken.None);

        Assert.Equal(newImg, result.ImageData);
        Assert.Equal("image/png", result.ImageContentType);
    }

    [Fact]
    public async Task UpdatePersonAsync_RemoveImage_ClearsImage()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = new byte[] { 1 }, ImageContentType = "image/jpeg"
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var cmd = new UpdatePersonCommand(personId, groupId, "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), null, null, true, null);

        var result = await _sut.UpdatePersonAsync(cmd, CancellationToken.None);

        Assert.Null(result.ImageData);
        Assert.Null(result.ImageContentType);
    }

    [Fact]
    public async Task UpdatePersonAsync_NoImageChange_KeepsExisting()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existingImg = new byte[] { 1, 2, 3 };
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = existingImg, ImageContentType = "image/jpeg"
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var cmd = new UpdatePersonCommand(personId, groupId, "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), null, null, false, null);

        var result = await _sut.UpdatePersonAsync(cmd, CancellationToken.None);

        Assert.Equal(existingImg, result.ImageData);
        Assert.Equal("image/jpeg", result.ImageContentType);
    }

    [Fact]
    public async Task UpdatePersonAsync_SanitizesNamesOnUpdate()
    {
        // "B0b!" → "B0b", "Jon3s#" → "Jon3s"
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var cmd = new UpdatePersonCommand(personId, groupId, "B0b!", "Jon3s#", null,
            new DateOnly(2000, 1, 1), null, null, false, null);

        var result = await _sut.UpdatePersonAsync(cmd, CancellationToken.None);

        Assert.Equal("B0b", result.FirstName);
        Assert.Equal("Jon3s", result.LastName);
    }

    [Fact]
    public async Task UpdatePersonAsync_ValidEmail_UpdatesEmail()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var cmd = new UpdatePersonCommand(personId, groupId, "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), null, null, false, "alice@example.com");

        var result = await _sut.UpdatePersonAsync(cmd, CancellationToken.None);

        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public async Task UpdatePersonAsync_InvalidEmail_Throws()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var cmd = new UpdatePersonCommand(personId, groupId, "Alice", "Smith", null,
            new DateOnly(2000, 1, 1), null, null, false, "bad-email");

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdatePersonAsync(cmd, CancellationToken.None));
    }

    // ── GetAllByGroupAsync ──────────────────────────────

    [Fact]
    public async Task GetAllByGroupAsync_ReturnsAllPersons()
    {
        var groupId = Guid.NewGuid();
        var people = new List<Person>
        {
            new() { Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Alice", LastName = "Smith",
                     BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Bob", LastName = "Jones",
                     BirthDate = new DateOnly(1995, 6, 15), CreatedAtUtc = DateTime.UtcNow },
        };
        _repoMock.Setup(r => r.GetByGroupIdAsync(groupId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(people);

        var result = await _sut.GetAllByGroupAsync(groupId, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllByGroupAsync_EmptyGroup_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetByGroupIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Person>());

        var result = await _sut.GetAllByGroupAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(result);
    }

    // ── GetUpcomingBirthdaysAsync ──────────────────────

    [Fact]
    public async Task GetUpcomingBirthdaysAsync_EmptyGroup_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetByGroupIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Person>());

        var result = await _sut.GetUpcomingBirthdaysAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUpcomingBirthdaysAsync_WithPreferredName_UsesIt()
    {
        var groupId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcoming = today.AddDays(5);
        var person = new Person
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            FirstName = "Robert",
            LastName = "Smith",
            PreferredName = "Bobby",
            BirthDate = new DateOnly(1990, upcoming.Month, upcoming.Day),
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByGroupIdAsync(groupId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Person> { person });

        var result = await _sut.GetUpcomingBirthdaysAsync(groupId, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Bobby", result[0].DisplayName);
    }

    [Fact]
    public async Task GetUpcomingBirthdaysAsync_WithoutPreferredName_CombinesFirstLast()
    {
        var groupId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcoming = today.AddDays(5);
        var person = new Person
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            FirstName = "Robert",
            LastName = "Smith",
            PreferredName = null,
            BirthDate = new DateOnly(1990, upcoming.Month, upcoming.Day),
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByGroupIdAsync(groupId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Person> { person });

        var result = await _sut.GetUpcomingBirthdaysAsync(groupId, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Robert Smith", result[0].DisplayName);
    }

    [Fact]
    public async Task GetUpcomingBirthdaysAsync_PersonWithImage_HasImageTrue()
    {
        var groupId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcoming = today.AddDays(5);
        var person = new Person
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            FirstName = "Alice",
            LastName = "Smith",
            BirthDate = new DateOnly(1990, upcoming.Month, upcoming.Day),
            ImageData = new byte[] { 1, 2, 3 },
            ImageContentType = "image/png",
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByGroupIdAsync(groupId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Person> { person });

        var result = await _sut.GetUpcomingBirthdaysAsync(groupId, CancellationToken.None);

        Assert.Single(result);
        Assert.True(result[0].HasImage);
    }

    [Fact]
    public async Task GetUpcomingBirthdaysAsync_PersonWithoutImage_HasImageFalse()
    {
        var groupId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcoming = today.AddDays(5);
        var person = new Person
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            FirstName = "Alice",
            LastName = "Smith",
            BirthDate = new DateOnly(1990, upcoming.Month, upcoming.Day),
            CreatedAtUtc = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByGroupIdAsync(groupId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Person> { person });

        var result = await _sut.GetUpcomingBirthdaysAsync(groupId, CancellationToken.None);

        Assert.Single(result);
        Assert.False(result[0].HasImage);
    }

    // ── DeletePersonAsync ──────────────────────────────

    [Fact]
    public async Task DeletePersonAsync_Valid_DeletesAndSaves()
    {
        var groupId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        await _sut.DeletePersonAsync(groupId, personId, CancellationToken.None);

        _repoMock.Verify(r => r.DeleteAsync(personId, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePersonAsync_PersonNotFound_ThrowsKeyNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Person?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.DeletePersonAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task DeletePersonAsync_WrongGroup_ThrowsKeyNotFound()
    {
        var personId = Guid.NewGuid();
        var existing = new Person
        {
            Id = personId, GroupId = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.DeletePersonAsync(Guid.NewGuid(), personId, CancellationToken.None));
    }

    [Fact]
    public async Task GetUpcomingBirthdaysAsync_LeapYearBirthday_DoesNotThrow()
    {
        var groupId = Guid.NewGuid();
        var person = new Person
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            FirstName = "Alice",
            LastName = "Smith",
            BirthDate = new DateOnly(2000, 2, 29), // leap year birthday
            CreatedAtUtc = DateTime.UtcNow
        };
        _repoMock.Setup(r => r.GetByGroupIdAsync(groupId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Person> { person });

        // Should not throw regardless of current date (leap year handling exercised)
        var result = await _sut.GetUpcomingBirthdaysAsync(groupId, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUpcomingBirthdaysAsync_ReturnsWithin60DayWindow()
    {
        var groupId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var within = today.AddDays(30);
        var beyond = today.AddDays(90);

        var people = new List<Person>
        {
            new() { Id = Guid.NewGuid(), GroupId = groupId, FirstName = "In", LastName = "Window",
                     BirthDate = new DateOnly(1990, within.Month, within.Day), CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Out", LastName = "Window",
                     BirthDate = new DateOnly(1990, beyond.Month, beyond.Day), CreatedAtUtc = DateTime.UtcNow },
        };

        _repoMock.Setup(r => r.GetByGroupIdAsync(groupId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(people);

        var result = await _sut.GetUpcomingBirthdaysAsync(groupId, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("In Window", result[0].DisplayName);
    }
}
