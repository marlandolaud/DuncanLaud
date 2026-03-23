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

        var result = await _sut.GetOrCreateGroupAsync(id, "  Smith Family  ", CancellationToken.None);

        Assert.Equal(id, result.Id);
        Assert.Equal("Smith Family", result.Name); // trimmed
        Assert.True(result.CreatedAtUtc <= DateTime.UtcNow);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateGroupAsync_TrimsName()
    {
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
}
