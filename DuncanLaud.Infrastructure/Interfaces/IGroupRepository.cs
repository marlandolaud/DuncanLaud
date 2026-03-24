using DuncanLaud.Infrastructure.Entities;

namespace DuncanLaud.Infrastructure.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Group group, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
