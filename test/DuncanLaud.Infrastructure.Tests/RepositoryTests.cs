using DuncanLaud.Infrastructure.Data;
using DuncanLaud.Infrastructure.Entities;
using DuncanLaud.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DuncanLaud.Infrastructure.Tests;

public class RepositoryTests : IDisposable
{
    private readonly AppDbContext _db;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
    }

    public void Dispose() => _db.Dispose();

    // ── GroupRepository ────────────────────────────────

    [Fact]
    public async Task GroupRepo_GetByIdAsync_ExistingGroup_ReturnsWithMembers()
    {
        var repo = new GroupRepository(_db);
        var group = new Group { Id = Guid.NewGuid(), Name = "Test", CreatedAtUtc = DateTime.UtcNow };
        _db.Groups.Add(group);
        _db.Persons.Add(new Person
        {
            Id = Guid.NewGuid(), GroupId = group.Id, FirstName = "Al", LastName = "Sm",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await repo.GetByIdAsync(group.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Test", result!.Name);
        Assert.Single(result.Members);
    }

    [Fact]
    public async Task GroupRepo_GetByIdAsync_NonExistentGroup_ReturnsNull()
    {
        var repo = new GroupRepository(_db);
        var result = await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task GroupRepo_AddAsync_AddsGroupToContext()
    {
        var repo = new GroupRepository(_db);
        var group = new Group { Id = Guid.NewGuid(), Name = "New", CreatedAtUtc = DateTime.UtcNow };

        await repo.AddAsync(group, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        var found = await _db.Groups.FindAsync(group.Id);
        Assert.NotNull(found);
        Assert.Equal("New", found!.Name);
    }

    [Fact]
    public async Task GroupRepo_SaveChangesAsync_PersistsChanges()
    {
        var repo = new GroupRepository(_db);
        var group = new Group { Id = Guid.NewGuid(), Name = "Save", CreatedAtUtc = DateTime.UtcNow };
        await repo.AddAsync(group, CancellationToken.None);

        await repo.SaveChangesAsync(CancellationToken.None);

        var count = await _db.Groups.CountAsync();
        Assert.Equal(1, count);
    }

    // ── PersonRepository ───────────────────────────────

    [Fact]
    public async Task PersonRepo_AddAsync_AddsPersonToContext()
    {
        var repo = new PersonRepository(_db);
        var groupId = Guid.NewGuid();
        _db.Groups.Add(new Group { Id = groupId, Name = "G", CreatedAtUtc = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var person = new Person
        {
            Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Al", LastName = "Sm",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        };

        await repo.AddAsync(person, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        var found = await _db.Persons.FindAsync(person.Id);
        Assert.NotNull(found);
    }

    [Fact]
    public async Task PersonRepo_GetByIdAsync_ExistingPerson_ReturnsPerson()
    {
        var repo = new PersonRepository(_db);
        var groupId = Guid.NewGuid();
        _db.Groups.Add(new Group { Id = groupId, Name = "G", CreatedAtUtc = DateTime.UtcNow });
        var personId = Guid.NewGuid();
        _db.Persons.Add(new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Al", LastName = "Sm",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = new byte[] { 1, 2, 3 }, ImageContentType = "image/png"
        });
        await _db.SaveChangesAsync();

        var result = await repo.GetByIdAsync(personId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Al", result!.FirstName);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.ImageData);
    }

    [Fact]
    public async Task PersonRepo_GetByIdAsync_NonExistentPerson_ReturnsNull()
    {
        var repo = new PersonRepository(_db);
        var result = await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task PersonRepo_GetByGroupIdAsync_ReturnsMatchingPersons()
    {
        var repo = new PersonRepository(_db);
        var groupId = Guid.NewGuid();
        var otherGroupId = Guid.NewGuid();
        _db.Groups.Add(new Group { Id = groupId, Name = "G1", CreatedAtUtc = DateTime.UtcNow });
        _db.Groups.Add(new Group { Id = otherGroupId, Name = "G2", CreatedAtUtc = DateTime.UtcNow });
        _db.Persons.Add(new Person
        {
            Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Al", LastName = "Sm",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        });
        _db.Persons.Add(new Person
        {
            Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Bo", LastName = "Jo",
            BirthDate = new DateOnly(2001, 2, 2), CreatedAtUtc = DateTime.UtcNow
        });
        _db.Persons.Add(new Person
        {
            Id = Guid.NewGuid(), GroupId = otherGroupId, FirstName = "Other", LastName = "Gr",
            BirthDate = new DateOnly(2002, 3, 3), CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await repo.GetByGroupIdAsync(groupId, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(groupId, p.GroupId));
    }

    [Fact]
    public async Task PersonRepo_GetByGroupIdAsync_NoMembers_ReturnsEmpty()
    {
        var repo = new PersonRepository(_db);
        var result = await repo.GetByGroupIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Empty(result);
    }

    [Fact]
    public async Task PersonRepo_GetByGroupIdAsync_ReturnsIReadOnlyList()
    {
        var repo = new PersonRepository(_db);
        var result = await repo.GetByGroupIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.IsAssignableFrom<IReadOnlyList<Person>>(result);
    }

    [Fact]
    public async Task PersonRepo_DeleteAsync_ExistingPerson_RemovesPerson()
    {
        var repo = new PersonRepository(_db);
        var groupId = Guid.NewGuid();
        _db.Groups.Add(new Group { Id = groupId, Name = "G", CreatedAtUtc = DateTime.UtcNow });
        var personId = Guid.NewGuid();
        _db.Persons.Add(new Person
        {
            Id = personId, GroupId = groupId, FirstName = "Al", LastName = "Sm",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await repo.DeleteAsync(personId, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        Assert.Null(await _db.Persons.FindAsync(personId));
    }

    [Fact]
    public async Task PersonRepo_DeleteAsync_NonExistentPerson_DoesNotThrow()
    {
        var repo = new PersonRepository(_db);
        // Should not throw even if the ID doesn't exist
        await repo.DeleteAsync(Guid.NewGuid(), CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);
        Assert.Equal(0, await _db.Persons.CountAsync());
    }

    // ── AppDbContext configuration tests ───────────────

    [Fact]
    public async Task DbContext_PersonCascadeDelete_RemovesPersonsWhenGroupDeleted()
    {
        var groupId = Guid.NewGuid();
        _db.Groups.Add(new Group { Id = groupId, Name = "G", CreatedAtUtc = DateTime.UtcNow });
        _db.Persons.Add(new Person
        {
            Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Al", LastName = "Sm",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var group = await _db.Groups.FindAsync(groupId);
        _db.Groups.Remove(group!);
        await _db.SaveChangesAsync();

        Assert.Empty(await _db.Persons.ToListAsync());
    }

    [Fact]
    public async Task DbContext_NullableFields_AllowNull()
    {
        var groupId = Guid.NewGuid();
        _db.Groups.Add(new Group { Id = groupId, Name = "G", CreatedAtUtc = DateTime.UtcNow });
        var person = new Person
        {
            Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Al", LastName = "Sm",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            PreferredName = null,
            ImageData = null,
            ImageContentType = null
        };
        _db.Persons.Add(person);
        await _db.SaveChangesAsync();

        var found = await _db.Persons.FindAsync(person.Id);
        Assert.Null(found!.PreferredName);
        Assert.Null(found.ImageData);
        Assert.Null(found.ImageContentType);
    }

    [Fact]
    public async Task DbContext_ImageDataPersistence_RoundTrips()
    {
        var groupId = Guid.NewGuid();
        _db.Groups.Add(new Group { Id = groupId, Name = "G", CreatedAtUtc = DateTime.UtcNow });
        var imgData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG magic bytes
        var person = new Person
        {
            Id = Guid.NewGuid(), GroupId = groupId, FirstName = "Al", LastName = "Sm",
            BirthDate = new DateOnly(2000, 1, 1), CreatedAtUtc = DateTime.UtcNow,
            ImageData = imgData,
            ImageContentType = "image/jpeg"
        };
        _db.Persons.Add(person);
        await _db.SaveChangesAsync();

        var found = await _db.Persons.FindAsync(person.Id);
        Assert.Equal(imgData, found!.ImageData);
        Assert.Equal("image/jpeg", found.ImageContentType);
    }
}
