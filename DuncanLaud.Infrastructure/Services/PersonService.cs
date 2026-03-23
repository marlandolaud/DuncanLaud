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

    public async Task<Person> UpdatePersonAsync(UpdatePersonCommand command, CancellationToken ct)
    {
        var person = await _personRepo.GetByIdAsync(command.PersonId, ct)
            ?? throw new KeyNotFoundException($"Person {command.PersonId} not found.");

        if (person.GroupId != command.GroupId)
            throw new KeyNotFoundException($"Person {command.PersonId} does not belong to group {command.GroupId}.");

        ValidateFields(command.FirstName, command.LastName, command.PreferredName, command.BirthDate);

        person.FirstName = command.FirstName.Trim();
        person.LastName = command.LastName.Trim();
        person.PreferredName = command.PreferredName?.Trim();
        person.BirthDate = command.BirthDate;

        if (command.RemoveImage)
        {
            person.ImageData = null;
            person.ImageContentType = null;
        }
        else if (command.ImageData is not null)
        {
            person.ImageData = command.ImageData;
            person.ImageContentType = command.ImageContentType;
        }
        // else: keep existing image unchanged

        await _personRepo.SaveChangesAsync(ct);
        return person;
    }

    public async Task<IReadOnlyList<Person>> GetAllByGroupAsync(Guid groupId, CancellationToken ct)
    {
        return await _personRepo.GetByGroupIdAsync(groupId, ct);
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
        => ValidateFields(command.FirstName, command.LastName, command.PreferredName, command.BirthDate);

    private static void ValidateFields(string firstName, string lastName, string? preferredName, DateOnly birthDate)
    {
        if (!PersonValidator.IsValidName(firstName))
            throw new ArgumentException("First name must be between 2 and 100 characters.", nameof(firstName));

        if (!PersonValidator.IsValidName(lastName))
            throw new ArgumentException("Last name must be between 2 and 100 characters.", nameof(lastName));

        if (preferredName is not null && !PersonValidator.IsValidName(preferredName, required: false))
            throw new ArgumentException("Preferred name must be between 2 and 100 characters.", nameof(preferredName));

        if (!PersonValidator.IsValidBirthDate(birthDate))
            throw new ArgumentException("Birth date must be in the past and no earlier than 1900.", nameof(birthDate));

        CheckProfanity(firstName, "First name");
        CheckProfanity(lastName, "Last name");
        if (preferredName is not null)
            CheckProfanity(preferredName, "Preferred name");
    }

    private static void CheckProfanity(string value, string fieldName)
    {
        if (ProfanityChecker.IsProfanity(value))
            throw new ArgumentException($"{fieldName} contains inappropriate content.", fieldName);
    }
}
