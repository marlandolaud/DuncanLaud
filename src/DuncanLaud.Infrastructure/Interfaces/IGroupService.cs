using DuncanLaud.Infrastructure.Entities;

namespace DuncanLaud.Infrastructure.Interfaces;

public interface IGroupService
{
    Task<Group> GetOrCreateGroupAsync(Guid groupId, string name, CancellationToken ct = default);
    Task<Group?> GetGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<Group> UpdateGroupNameAsync(Guid groupId, string name, CancellationToken ct = default);
}
