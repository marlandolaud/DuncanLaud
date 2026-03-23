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
        var response = await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Test Family"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOpts);
        Assert.NotNull(body);
        Assert.Equal(groupId, body!.Id);
        Assert.Equal("Test Family", body.Name);
        Assert.Equal(0, body.MemberCount);
    }

    [Fact]
    public async Task CreateGroup_SameId_ReturnsExisting()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "First Name"));
        var response = await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Second Name"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOpts);
        Assert.Equal("First Name", body!.Name);
    }

    // ── GetGroup ───────────────────────────────────────

    [Fact]
    public async Task GetGroup_Exists_Returns200()
    {
        using var client = CreateClient();
        var groupId = Guid.NewGuid();
        await client.PostAsJsonAsync("/api/group", new CreateGroupRequest(groupId, "Get Test"));

        var response = await client.GetAsync($"/api/group/{groupId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GroupResponse>(JsonOpts);
        Assert.Equal("Get Test", body!.Name);
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

                // Disable rate limiting for tests
                services.Configure<IpRateLimitOptions>(opts =>
                {
                    opts.GeneralRules.Clear();
                });
            });
        }
    }
}
