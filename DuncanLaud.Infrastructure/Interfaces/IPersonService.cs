using DuncanLaud.Domain.Commands;
using DuncanLaud.Domain.ValueObjects;
using DuncanLaud.Infrastructure.Entities;

namespace DuncanLaud.Infrastructure.Interfaces;

public interface IPersonService
{
    Task<Person> AddPersonAsync(CreatePersonCommand command, CancellationToken ct = default);
    Task<Person> UpdatePersonAsync(UpdatePersonCommand command, CancellationToken ct = default);
    Task<IReadOnlyList<Person>> GetAllByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<IReadOnlyList<BirthdayResult>> GetUpcomingBirthdaysAsync(Guid groupId, CancellationToken ct = default);
}
