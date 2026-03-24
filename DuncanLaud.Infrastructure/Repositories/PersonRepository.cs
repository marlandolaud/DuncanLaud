using DuncanLaud.Infrastructure.Data;
using DuncanLaud.Infrastructure.Entities;
using DuncanLaud.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuncanLaud.Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;

    public PersonRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Person person, CancellationToken ct)
        => await _db.Persons.AddAsync(person, ct);

    public Task<Person?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Persons.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<IReadOnlyList<Person>> GetByGroupIdAsync(Guid groupId, CancellationToken ct)
        => _db.Persons
              .Where(p => p.GroupId == groupId)
              .ToListAsync(ct)
              .ContinueWith(t => (IReadOnlyList<Person>)t.Result, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
