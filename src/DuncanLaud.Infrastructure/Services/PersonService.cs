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
        var sanitizedFirst = PersonValidator.Sanitize(command.FirstName);
        var sanitizedLast  = PersonValidator.Sanitize(command.LastName);
        var sanitizedPref  = command.PreferredName is null ? null : PersonValidator.Sanitize(command.PreferredName);

        ValidateFields(sanitizedFirst, sanitizedLast, sanitizedPref, command.BirthDate);

        ValidateEmail(command.Email);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            GroupId = command.GroupId,
            FirstName = sanitizedFirst,
            LastName = sanitizedLast,
            PreferredName = string.IsNullOrEmpty(sanitizedPref) ? null : sanitizedPref,
            BirthDate = command.BirthDate,
            ImageData = command.ImageData,
            ImageContentType = command.ImageContentType,
            Email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim(),
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

        var sanitizedFirst = PersonValidator.Sanitize(command.FirstName);
        var sanitizedLast  = PersonValidator.Sanitize(command.LastName);
        var sanitizedPref  = command.PreferredName is null ? null : PersonValidator.Sanitize(command.PreferredName);

        ValidateFields(sanitizedFirst, sanitizedLast, sanitizedPref, command.BirthDate);
        ValidateEmail(command.Email);

        person.FirstName = sanitizedFirst;
        person.LastName = sanitizedLast;
        person.PreferredName = string.IsNullOrEmpty(sanitizedPref) ? null : sanitizedPref;
        person.BirthDate = command.BirthDate;
        person.Email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim();

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

    public async Task DeletePersonAsync(Guid groupId, Guid personId, CancellationToken ct)
    {
        var person = await _personRepo.GetByIdAsync(personId, ct)
            ?? throw new KeyNotFoundException($"Person {personId} not found.");

        if (person.GroupId != groupId)
            throw new KeyNotFoundException($"Person {personId} does not belong to group {groupId}.");

        await _personRepo.DeleteAsync(personId, ct);
        await _personRepo.SaveChangesAsync(ct);
    }

    private static void ValidateFields(string firstName, string lastName, string? preferredName, DateOnly birthDate)
    {
        if (!PersonValidator.IsValidName(firstName))
            throw new ArgumentException("First name must be 2–100 letters and numbers only.", nameof(firstName));

        if (!PersonValidator.IsValidName(lastName))
            throw new ArgumentException("Last name must be 2–100 letters and numbers only.", nameof(lastName));

        if (!string.IsNullOrEmpty(preferredName) && !PersonValidator.IsValidName(preferredName, required: false))
            throw new ArgumentException("Preferred name must be 2–100 letters and numbers only.", nameof(preferredName));

        if (!PersonValidator.IsValidBirthDate(birthDate))
            throw new ArgumentException("Birth date must be in the past and no earlier than 1900.", nameof(birthDate));

        CheckProfanity(firstName, "First name");
        CheckProfanity(lastName, "Last name");
        if (!string.IsNullOrEmpty(preferredName))
            CheckProfanity(preferredName, "Preferred name");
    }

    private static void CheckProfanity(string value, string fieldName)
    {
        if (ProfanityChecker.IsProfanity(value))
            throw new ArgumentException($"{fieldName} contains inappropriate content.", fieldName);
    }

    private static void ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        var trimmed = email.Trim();
        if (trimmed.Length > 254)
            throw new ArgumentException("Email must be 254 characters or fewer.", nameof(email));

        try
        {
            var addr = new System.Net.Mail.MailAddress(trimmed);
            if (addr.Address != trimmed)
                throw new ArgumentException("Email address is not valid.", nameof(email));
        }
        catch (FormatException)
        {
            throw new ArgumentException("Email address is not valid.", nameof(email));
        }
    }
}
