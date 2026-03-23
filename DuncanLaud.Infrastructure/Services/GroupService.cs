using DuncanLaud.Infrastructure.Entities;
using DuncanLaud.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProfanityFilter;

namespace DuncanLaud.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepo;
    private static readonly ProfanityFilter.ProfanityFilter ProfanityChecker = new();

    public GroupService(IGroupRepository groupRepo) => _groupRepo = groupRepo;

    public async Task<Group?> GetGroupAsync(Guid groupId, CancellationToken ct)
        => await _groupRepo.GetByIdAsync(groupId, ct);

    public async Task<Group> GetOrCreateGroupAsync(Guid groupId, string name, CancellationToken ct)
    {
        var existing = await _groupRepo.GetByIdAsync(groupId, ct);
        if (existing is not null)
            return existing;

        var trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            throw new ArgumentException("Group name is required.", nameof(name));

        if (trimmedName.Length < 2 || trimmedName.Length > 100)
            throw new ArgumentException("Group name must be between 2 and 100 characters.", nameof(name));

        if (ProfanityChecker.IsProfanity(trimmedName))
            throw new ArgumentException("Group name contains inappropriate content.", nameof(name));

        var group = new Group
        {
            Id = groupId,
            Name = name.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        try
        {
            await _groupRepo.AddAsync(group, ct);
            await _groupRepo.SaveChangesAsync(ct);
            return group;
        }
        catch (DbUpdateException)
        {
            // Concurrent insert (e.g. React StrictMode double-fire) — return the winner's row
            return (await _groupRepo.GetByIdAsync(groupId, ct))!;
        }
    }
}
