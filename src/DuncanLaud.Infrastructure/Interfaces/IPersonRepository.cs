using DuncanLaud.Infrastructure.Entities;

namespace DuncanLaud.Infrastructure.Interfaces;

public interface IPersonRepository
{
    Task AddAsync(Person person, CancellationToken ct = default);
    Task<Person?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Person>> GetByGroupIdAsync(Guid groupId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
