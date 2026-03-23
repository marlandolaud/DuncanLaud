using DuncanLaud.Infrastructure.Data;
using DuncanLaud.Infrastructure.Entities;
using DuncanLaud.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuncanLaud.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _db;

    public GroupRepository(AppDbContext db) => _db = db;

    public Task<Group?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Groups.Include(g => g.Members)
                     .FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task AddAsync(Group group, CancellationToken ct)
        => await _db.Groups.AddAsync(group, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
