using DuncanLaud.Domain.Commands;
using DuncanLaud.Domain.Services;
using DuncanLaud.Domain.ValueObjects;
using DuncanLaud.Infrastructure.Entities;
using DuncanLaud.Infrastructure.Interfaces;
using ProfanityFilter;

namespace DuncanLaud.Infrastructure.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepo;
    private static readonly ProfanityFilter.ProfanityFilter ProfanityChecker = new();
    private const int BirthdayWindowDays = 60;

    public PersonService(IPersonRepository personRepo) => _personRepo = personRepo;

    public async Task<Person> AddPersonAsync(CreatePersonCommand command, CancellationToken ct)
    {
        ValidateCommand(command);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            GroupId = command.GroupId,
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            PreferredName = command.PreferredName?.Trim(),
            BirthDate = command.BirthDate,
            ImageData = command.ImageData,
            ImageContentType = command.ImageContentType,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _personRepo.AddAsync(person, ct);
        await _personRepo.SaveChangesAsync(ct);
        return person;
    }

    public async Task<IReadOnlyList<BirthdayResult>> GetUpcomingBirthdaysAsync(Guid groupId, CancellationToken ct)
    {
        var members = await _personRepo.GetByGroupIdAsync(groupId, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return BirthdayCalculator.GetUpcoming(
            members,
            p => p.Id,
            p => p.PreferredName ?? $"{p.FirstName} {p.LastName}",
            p => p.BirthDate,
            p => p.ImageData is not null,
            BirthdayWindowDays,
            today);
    }

    private static void ValidateCommand(CreatePersonCommand command)
    {
        if (!PersonValidator.IsValidName(command.FirstName))
            throw new ArgumentException("First name must be between 2 and 100 characters.", nameof(command.FirstName));

        if (!PersonValidator.IsValidName(command.LastName))
            throw new ArgumentException("Last name must be between 2 and 100 characters.", nameof(command.LastName));

        if (command.PreferredName is not null && !PersonValidator.IsValidName(command.PreferredName, required: false))
            throw new ArgumentException("Preferred name must be between 2 and 100 characters.", nameof(command.PreferredName));

        if (!PersonValidator.IsValidBirthDate(command.BirthDate))
            throw new ArgumentException("Birth date must be in the past and no earlier than 1900.", nameof(command.BirthDate));

        CheckProfanity(command.FirstName, "First name");
        CheckProfanity(command.LastName, "Last name");
        if (command.PreferredName is not null)
            CheckProfanity(command.PreferredName, "Preferred name");
    }

    private static void CheckProfanity(string value, string fieldName)
    {
        if (ProfanityChecker.IsProfanity(value))
            throw new ArgumentException($"{fieldName} contains inappropriate content.", fieldName);
    }
}
