using DuncanLaud.Domain.Services;
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

        var sanitized = PersonValidator.Sanitize(name);

        if (string.IsNullOrWhiteSpace(sanitized))
            throw new ArgumentException("Group name is required.", nameof(name));

        if (sanitized.Length < 2 || sanitized.Length > 100)
            throw new ArgumentException("Group name must be between 2 and 100 characters.", nameof(name));

        if (!PersonValidator.IsValidName(sanitized))
            throw new ArgumentException("Group name may only contain letters (A-Z, a-z) and numbers (0-9).", nameof(name));

        if (ProfanityChecker.IsProfanity(sanitized))
            throw new ArgumentException("Group name contains inappropriate content.", nameof(name));

        var group = new Group
        {
            Id = groupId,
            Name = sanitized,
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

    public async Task<Group> UpdateGroupNameAsync(Guid groupId, string name, CancellationToken ct)
    {
        var group = await _groupRepo.GetByIdAsync(groupId, ct)
            ?? throw new KeyNotFoundException($"Group {groupId} not found.");

        var sanitized = PersonValidator.Sanitize(name);

        if (string.IsNullOrWhiteSpace(sanitized))
            throw new ArgumentException("Group name is required.", nameof(name));

        if (sanitized.Length < 2 || sanitized.Length > 100)
            throw new ArgumentException("Group name must be between 2 and 100 characters.", nameof(name));

        if (!PersonValidator.IsValidName(sanitized))
            throw new ArgumentException("Group name may only contain letters (A-Z, a-z) and numbers (0-9).", nameof(name));

        if (ProfanityChecker.IsProfanity(sanitized))
            throw new ArgumentException("Group name contains inappropriate content.", nameof(name));

        group.Name = sanitized;
        await _groupRepo.SaveChangesAsync(ct);
        return group;
    }
}
