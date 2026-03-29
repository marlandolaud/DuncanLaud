using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AspNetCoreRateLimit;
using DuncanLaud.DTOs;
using DuncanLaud.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DuncanLaud.WebUI.Tests;

[Collection("Integration")]
public class IntegrationTests : IClassFixture<IntegrationTests.TestAppFactory>
{
    private readonly TestAppFactory _factory;

    public IntegrationTests(TestAppFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    // ── CreateGroup ────────────────────────────────────

    [Fact]
    public async Task CreateGroup_Returns201_AndPersistsGroup()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        var response = await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "TestFamily"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal(groupId, body!.Id);
        Assert.Equal("TestFamily", body.Name);
        Assert.Equal(0, body.MemberCount);
    }

    [Fact]
    public async Task CreateGroup_SameId_ReturnsExisting()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "FirstGroup"));
        var response = await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "SecondGroup"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOpts);
        Assert.Equal("FirstGroup", body!.Name);
    }

    // ── GetGroup ───────────────────────────────────────

    [Fact]
    public async Task GetGroup_Exists_Returns200()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "GetTest"));

        var response = await client.GetAsync($"/api/group/{groupId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOpts);
        Assert.Equal("GetTest", body!.Name);
    }

    [Fact]
    public async Task GetGroup_NotFound_Returns404()
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"/api/group/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── AddPerson ──────────────────────────────────────

    [Fact]
    public async Task AddPerson_ValidNoImage_Returns201()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Add Person Test"));

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };

        var response = await client.PostAsync($"/api/group/{groupId}/person", form);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.Equal("Alice", body!.FirstName);
        Assert.Equal("Smith", body.LastName);
        Assert.False(body.HasImage);
    }

    [Fact]
    public async Task AddPerson_WithImage_Returns201_HasImageTrue()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Image Test"));

        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Bob"), "firstName" },
            { new StringContent("Jones"), "lastName" },
            { new StringContent("1995-06-15"), "birthDate" },
            { imageContent, "image", "photo.jpg" }
        };

        var response = await client.PostAsync($"/api/group/{groupId}/person", form);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.True(body!.HasImage);
    }

    [Fact]
    public async Task AddPerson_GroupNotFound_Returns404()
    {
        using var client = CreateClient();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };

        var response = await client.PostAsync($"/api/group/{Guid.NewGuid()}/person", form);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddPerson_InvalidName_Returns400()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Validation Test"));

        using var form = new MultipartFormDataContent
        {
            { new StringContent("A"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };

        var response = await client.PostAsync($"/api/group/{groupId}/person", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddPerson_InvalidImageType_Returns400()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Bad Image Test"));

        var badContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        badContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" },
            { badContent, "image", "doc.pdf" }
        };

        var response = await client.PostAsync($"/api/group/{groupId}/person", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddPerson_WithPreferredName_Persisted()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Preferred Test"));

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Robert"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("Bobby"), "preferredName" },
            { new StringContent("1990-03-25"), "birthDate" }
        };

        var response = await client.PostAsync($"/api/group/{groupId}/person", form);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.Equal("Bobby", body!.PreferredName);
    }

    // ── GetPersonImage ─────────────────────────────────

    [Fact]
    public async Task GetPersonImage_Exists_ReturnsImage()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Image Serve Test"));

        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" },
            { imageContent, "image", "photo.jpg" }
        };

        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", form);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);

        var imageResponse = await client.GetAsync($"/api/group/{groupId}/person/{person!.Id}/image");

        Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
        Assert.Equal("image/jpeg", imageResponse.Content.Headers.ContentType!.MediaType);
        var body = await imageResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(imageBytes, body);
    }

    [Fact]
    public async Task GetPersonImage_NoImage_Returns404()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "No Image Test"));

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };

        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", form);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);

        var imageResponse = await client.GetAsync($"/api/group/{groupId}/person/{person!.Id}/image");

        Assert.Equal(HttpStatusCode.NotFound, imageResponse.StatusCode);
    }

    [Fact]
    public async Task GetPersonImage_WrongGroup_Returns404()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Wrong Group Test"));

        var imageContent = new ByteArrayContent(new byte[] { 1 });
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" },
            { imageContent, "image", "photo.png" }
        };

        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", form);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);

        var imageResponse = await client.GetAsync($"/api/group/{Guid.NewGuid()}/person/{person!.Id}/image");

        Assert.Equal(HttpStatusCode.NotFound, imageResponse.StatusCode);
    }

    // ── GetBirthdays ───────────────────────────────────

    [Fact]
    public async Task GetBirthdays_EmptyGroup_ReturnsEmptyArray()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Empty Birthday Test"));

        var response = await client.GetAsync($"/api/group/{groupId}/birthdays");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<BirthdayResponse>>(JsonOpts);
        Assert.NotNull(body);
        Assert.Empty(body!);
    }

    [Fact]
    public async Task GetBirthdays_GroupNotFound_Returns404()
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"/api/group/{Guid.NewGuid()}/birthdays");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBirthdays_WithUpcomingPerson_ReturnsBirthday()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Birthday Test"));

        var upcoming = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        var birthDate = new DateOnly(1990, upcoming.Month, upcoming.Day);

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent(birthDate.ToString("yyyy-MM-dd")), "birthDate" }
        };

        await client.PostAsync($"/api/group/{groupId}/person", form);

        var response = await client.GetAsync($"/api/group/{groupId}/birthdays");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<BirthdayResponse>>(JsonOpts);
        Assert.Single(body!);
        Assert.Equal("Alice Smith", body![0].DisplayName);
    }

    // ── GetPersons ────────────────────────────────────

    [Fact]
    public async Task GetPersons_ReturnsAllMembers()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "List Test"));

        using var form1 = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };
        using var form2 = new MultipartFormDataContent
        {
            { new StringContent("Bob"), "firstName" },
            { new StringContent("Jones"), "lastName" },
            { new StringContent("1995-06-15"), "birthDate" }
        };

        await client.PostAsync($"/api/group/{groupId}/person", form1);
        await client.PostAsync($"/api/group/{groupId}/person", form2);

        var response = await client.GetAsync($"/api/group/{groupId}/persons");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PersonResponse>>(JsonOpts);
        Assert.Equal(2, body!.Count);
    }

    [Fact]
    public async Task GetPersons_GroupNotFound_Returns404()
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"/api/group/{Guid.NewGuid()}/persons");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── GetPerson ─────────────────────────────────────

    [Fact]
    public async Task GetPerson_Exists_Returns200()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Get Person Test"));

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("Bobby"), "preferredName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };
        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", form);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);

        var response = await client.GetAsync($"/api/group/{groupId}/person/{person!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.Equal("Alice", body!.FirstName);
        Assert.Equal("Bobby", body.PreferredName);
    }

    [Fact]
    public async Task GetPerson_NotFound_Returns404()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "404 Person Test"));

        var response = await client.GetAsync($"/api/group/{groupId}/person/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── UpdatePerson ──────────────────────────────────

    [Fact]
    public async Task UpdatePerson_Valid_Returns200WithUpdatedData()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Update Test"));

        using var addForm = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };
        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", addForm);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);

        using var updateForm = new MultipartFormDataContent
        {
            { new StringContent("Bob"), "firstName" },
            { new StringContent("Jones"), "lastName" },
            { new StringContent("Bobby"), "preferredName" },
            { new StringContent("1995-06-15"), "birthDate" },
            { new StringContent("false"), "removeImage" }
        };

        var response = await client.PutAsync($"/api/group/{groupId}/person/{person!.Id}", updateForm);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.Equal("Bob", body!.FirstName);
        Assert.Equal("Jones", body.LastName);
        Assert.Equal("Bobby", body.PreferredName);
    }

    [Fact]
    public async Task UpdatePerson_WithNewImage_ReplacesImage()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Image Update"));

        using var addForm = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };
        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", addForm);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.False(person!.HasImage);

        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var updateForm = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" },
            { new StringContent("false"), "removeImage" },
            { imageContent, "image", "photo.png" }
        };

        var response = await client.PutAsync($"/api/group/{groupId}/person/{person.Id}", updateForm);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.True(body!.HasImage);

        // Verify image is served
        var imgResponse = await client.GetAsync($"/api/group/{groupId}/person/{person.Id}/image");
        Assert.Equal(HttpStatusCode.OK, imgResponse.StatusCode);
        var imgBody = await imgResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(imageBytes, imgBody);
    }

    [Fact]
    public async Task UpdatePerson_RemoveImage_ClearsImage()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Remove Image"));

        var imageBytes = new byte[] { 0xFF, 0xD8 };
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        using var addForm = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" },
            { imageContent, "image", "photo.jpg" }
        };
        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", addForm);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.True(person!.HasImage);

        using var updateForm = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" },
            { new StringContent("true"), "removeImage" }
        };

        var response = await client.PutAsync($"/api/group/{groupId}/person/{person.Id}", updateForm);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.False(body!.HasImage);

        // Verify image is gone
        var imgResponse = await client.GetAsync($"/api/group/{groupId}/person/{person.Id}/image");
        Assert.Equal(HttpStatusCode.NotFound, imgResponse.StatusCode);
    }

    [Fact]
    public async Task UpdatePerson_GroupNotFound_Returns404()
    {
        using var client = CreateClient();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" },
            { new StringContent("false"), "removeImage" }
        };

        var response = await client.PutAsync($"/api/group/{Guid.NewGuid()}/person/{Guid.NewGuid()}", form);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePerson_PersonNotFound_Returns404()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Missing Person"));

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" },
            { new StringContent("false"), "removeImage" }
        };

        var response = await client.PutAsync($"/api/group/{groupId}/person/{Guid.NewGuid()}", form);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── DeletePerson ─────────────────────────────────

    [Fact]
    public async Task DeletePerson_Valid_Returns204_AndPersonGone()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Delete Test"));

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent("2000-01-15"), "birthDate" }
        };
        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", form);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);

        var deleteResponse = await client.DeleteAsync($"/api/group/{groupId}/person/{person!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/group/{groupId}/person/{person.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeletePerson_PersonNotFound_Returns404()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Delete 404 Test"));

        var response = await client.DeleteAsync($"/api/group/{groupId}/person/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePerson_GroupNotFound_Returns404()
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"/api/group/{Guid.NewGuid()}/person/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── UpdateGroupName ───────────────────────────────

    [Fact]
    public async Task UpdateGroupName_Valid_Returns200WithNewName()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "OriginalName"));

        var response = await client.PatchAsJsonAsync($"/api/group/{groupId}/name", new { name = "UpdatedName" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOpts);
        Assert.Equal("UpdatedName", body!.Name);
    }

    [Fact]
    public async Task UpdateGroupName_GroupNotFound_Returns404()
    {
        using var client = CreateClient();
        var response = await client.PatchAsJsonAsync($"/api/group/{Guid.NewGuid()}/name", new { name = "NewName" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGroupName_InvalidName_Returns400()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "ValidGroup"));

        var response = await client.PatchAsJsonAsync($"/api/group/{groupId}/name", new { name = "shit" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGroupName_EmptyName_Returns400()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "ValidGroup"));

        var response = await client.PatchAsJsonAsync($"/api/group/{groupId}/name", new { name = "!@#" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeletePerson_ThenBirthdayListUpdates()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Birthday Delete Test"));

        var upcoming = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5);
        var birthDate = new DateOnly(1990, upcoming.Month, upcoming.Day);

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Alice"), "firstName" },
            { new StringContent("Smith"), "lastName" },
            { new StringContent(birthDate.ToString("yyyy-MM-dd")), "birthDate" }
        };
        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", form);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);

        // Verify birthday appears
        var birthdaysBefore = await (await client.GetAsync($"/api/group/{groupId}/birthdays"))
            .Content.ReadFromJsonAsync<List<BirthdayResponse>>(JsonOpts);
        Assert.Single(birthdaysBefore!);

        // Delete person
        await client.DeleteAsync($"/api/group/{groupId}/person/{person!.Id}");

        // Verify birthday list is now empty
        var birthdaysAfter = await (await client.GetAsync($"/api/group/{groupId}/birthdays"))
            .Content.ReadFromJsonAsync<List<BirthdayResponse>>(JsonOpts);
        Assert.Empty(birthdaysAfter!);
    }

    // ── Full flow ──────────────────────────────────────

    [Fact]
    public async Task FullFlow_CreateGroup_AddPerson_GetBirthdays_GetImage()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();

        // 1. Create group
        var createResponse = await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Full Flow"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // 2. Add person with image
        var upcoming = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5);
        var birthDate = new DateOnly(1990, upcoming.Month, upcoming.Day);
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var form = new MultipartFormDataContent
        {
            { new StringContent("Charlie"), "firstName" },
            { new StringContent("Brown"), "lastName" },
            { new StringContent("Chuck"), "preferredName" },
            { new StringContent(birthDate.ToString("yyyy-MM-dd")), "birthDate" },
            { imageContent, "image", "photo.png" }
        };

        var addResponse = await client.PostAsync($"/api/group/{groupId}/person", form);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var person = await addResponse.Content.ReadFromJsonAsync<PersonResponse>(JsonOpts);
        Assert.True(person!.HasImage);
        Assert.Equal("Chuck", person.PreferredName);

        // 3. Get group — verify member count
        var getResponse = await client.GetAsync($"/api/group/{groupId}");
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponse>(JsonOpts);
        Assert.Equal(1, group!.MemberCount);

        // 4. Get birthdays
        var birthdayResponse = await client.GetAsync($"/api/group/{groupId}/birthdays");
        var birthdays = await birthdayResponse.Content.ReadFromJsonAsync<List<BirthdayResponse>>(JsonOpts);
        Assert.Single(birthdays!);
        Assert.Equal("Chuck", birthdays![0].DisplayName);
        Assert.True(birthdays[0].HasImage);

        // 5. Get image
        var imageResponse = await client.GetAsync($"/api/group/{groupId}/person/{person.Id}/image");
        Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
        Assert.Equal("image/png", imageResponse.Content.Headers.ContentType!.MediaType);
    }

    // ── Helpers ─────────────────────────────────────────

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public class TestAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"IntegrationTestDb_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseContentRoot(Directory.GetCurrentDirectory());

            builder.ConfigureServices(services =>
            {
                // Remove all EF Core / SQL Server registrations
                var efDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                             || d.ServiceType == typeof(DbContextOptions)
                             || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                    .ToList();
                foreach (var d in efDescriptors)
                    services.Remove(d);

                // Add InMemory database for testing (unique per factory instance)
                var dbName = _dbName;
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
                services.AddDbContext<DuncanLaud.Analytics.AnalyticsDbContext>(options =>
                    options.UseInMemoryDatabase(dbName + "_analytics"));

                // Disable rate limiting for tests
                services.Configure<IpRateLimitOptions>(opts =>
                {
                    opts.GeneralRules.Clear();
                });
            });
        }
    }
}
