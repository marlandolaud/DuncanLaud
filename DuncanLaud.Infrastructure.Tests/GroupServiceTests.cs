using DuncanLaud.Infrastructure.Entities;
using DuncanLaud.Infrastructure.Interfaces;
using DuncanLaud.Infrastructure.Services;
using Moq;

namespace DuncanLaud.Infrastructure.Tests;

public class GroupServiceTests
{
    private readonly Mock<IGroupRepository> _repoMock = new();
    private readonly GroupService _sut;

    public GroupServiceTests() => _sut = new GroupService(_repoMock.Object);

    // ── GetGroupAsync ──────────────────────────────────

    [Fact]
    public async Task GetGroupAsync_GroupExists_ReturnsGroup()
    {
        var id = Guid.NewGuid();
        var group = new Group { Id = id, Name = "Test" };
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(group);

        var result = await _sut.GetGroupAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
    }

    [Fact]
    public async Task GetGroupAsync_GroupDoesNotExist_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var result = await _sut.GetGroupAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    // ── GetOrCreateGroupAsync ──────────────────────────

    [Fact]
    public async Task GetOrCreateGroupAsync_GroupExists_ReturnsExisting_DoesNotCreate()
    {
        var id = Guid.NewGuid();
        var existing = new Group { Id = id, Name = "Existing" };
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var result = await _sut.GetOrCreateGroupAsync(id, "NewName", CancellationToken.None);

        Assert.Equal("Existing", result.Name);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_GroupDoesNotExist_CreatesNew()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var result = await _sut.GetOrCreateGroupAsync(id, "SmithFamily", CancellationToken.None);

        Assert.Equal(id, result.Id);
        Assert.Equal("SmithFamily", result.Name);
        Assert.True(result.CreatedAtUtc <= DateTime.UtcNow);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_SanitizesName()
    {
        // "  test  " sanitized → "test"
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var result = await _sut.GetOrCreateGroupAsync(Guid.NewGuid(), "  test  ", CancellationToken.None);

        Assert.Equal("test", result.Name);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_SetsCreatedAtUtc()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var before = DateTime.UtcNow;
        var result = await _sut.GetOrCreateGroupAsync(Guid.NewGuid(), "Test", CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.InRange(result.CreatedAtUtc, before, after);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_ProfaneName_Throws()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetOrCreateGroupAsync(Guid.NewGuid(), "shit", CancellationToken.None));

        Assert.Contains("inappropriate", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetOrCreateGroupAsync_EmptyName_Throws(string name)
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetOrCreateGroupAsync(Guid.NewGuid(), name, CancellationToken.None));

        Assert.Contains("required", ex.Message);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_NameTooShort_Throws()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetOrCreateGroupAsync(Guid.NewGuid(), "A", CancellationToken.None));

        Assert.Contains("between 2 and 100", ex.Message);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_NameTooLong_Throws()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetOrCreateGroupAsync(Guid.NewGuid(), new string('A', 101), CancellationToken.None));

        Assert.Contains("between 2 and 100", ex.Message);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_ExistingGroup_SkipsValidation()
    {
        var id = Guid.NewGuid();
        var existing = new Group { Id = id, Name = "Existing" };
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        // Profane name shouldn't matter since group already exists
        var result = await _sut.GetOrCreateGroupAsync(id, "shit", CancellationToken.None);

        Assert.Equal("Existing", result.Name);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_NameWithSpecialCharsOnly_Throws()
    {
        // "!@#$%" sanitized to "" → throws "required"
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetOrCreateGroupAsync(Guid.NewGuid(), "!@#$%", CancellationToken.None));

        Assert.Contains("required", ex.Message);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_AlphanumericName_Succeeds()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Group?)null);

        var result = await _sut.GetOrCreateGroupAsync(Guid.NewGuid(), "BirthdayGroup2025", CancellationToken.None);

        Assert.Equal("BirthdayGroup2025", result.Name);
    }
}
