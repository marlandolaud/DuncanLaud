using DuncanLaud.Analytics.Entities;
using DuncanLaud.Infrastructure.Entities;

namespace DuncanLaud.Infrastructure.Tests;

/// <summary>
/// Exercises navigation-property setters on entity classes.
/// EF Core populates these via reflection at runtime; unit tests
/// must set them explicitly to keep coverage above the threshold.
/// </summary>
public class EntityNavigationPropertyTests
{
    [Fact]
    public void Person_Group_NavigationProperty_CanBeSetAndRead()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "TestGroup", CreatedAtUtc = DateTime.UtcNow };
        var person = new Person
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            FirstName = "Alice",
            LastName = "Smith",
            BirthDate = new DateOnly(1990, 1, 1),
            Group = group
        };

        Assert.Same(group, person.Group);
    }

    [Fact]
    public void ApiEvent_Session_NavigationProperty_CanBeSetAndRead()
    {
        var session = new Session
        {
            IpHash = "abc123",
            IpPrefix = "1.2.3",
            UserAgentHash = "def456",
            SessionStart = DateTime.UtcNow
        };

        var apiEvent = new ApiEvent
        {
            Endpoint = "/api/group",
            HttpMethod = "GET",
            StatusCode = 200,
            Session = session
        };

        Assert.Same(session, apiEvent.Session);
    }

    [Fact]
    public void PageEvent_Session_NavigationProperty_CanBeSetAndRead()
    {
        var session = new Session
        {
            IpHash = "abc123",
            IpPrefix = "1.2.3",
            UserAgentHash = "def456",
            SessionStart = DateTime.UtcNow
        };

        var pageEvent = new PageEvent
        {
            UrlPath = "/home",
            HttpMethod = "GET",
            StatusCode = 200,
            Session = session
        };

        Assert.Same(session, pageEvent.Session);
    }

    [Fact]
    public void Session_NavigationCollections_AreInitialisedEmpty()
    {
        var session = new Session
        {
            IpHash = "abc",
            IpPrefix = "1.2.3",
            UserAgentHash = "def",
            SessionStart = DateTime.UtcNow
        };

        Assert.NotNull(session.PageEvents);
        Assert.NotNull(session.ApiEvents);
        Assert.Empty(session.PageEvents);
        Assert.Empty(session.ApiEvents);
    }
}
