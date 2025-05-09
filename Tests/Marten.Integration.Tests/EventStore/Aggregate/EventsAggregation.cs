using FluentAssertions;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate;

public class EventsAggregation(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer)
{
    public record IssueCreated(
        Guid IssueId,
        string Description
    );

    public record IssueUpdated(
        Guid IssueId,
        string Description
    );

    public record IssueRemoved(
        Guid IssueId
    );

    public record Issue(
        Guid IssueId,
        string Description
    );

    public class IssuesList
    {
        public Guid Id { get; set; }
        public Dictionary<Guid, Issue> Issues { get; } = [];

        public void Apply(IssueCreated @event)
        {
            var (issueId, description) = @event;
            Issues.Add(issueId, new Issue(issueId, description));
        }

        public void Apply(IssueUpdated @event)
        {
            if (!Issues.TryGetValue(@event.IssueId, out Issue? value))
                return;

            Issues[@event.IssueId] = value with {Description = @event.Description};
        }

        public void Apply(IssueRemoved @event)
        {
            Issues.Remove(@event.IssueId);
        }
    }

    [Fact]
    public async Task GivenStreamOfEvents_WhenAggregateStreamIsCalled_ThenChangesAreAppliedProperly()
    {
        var streamId = Guid.NewGuid();

        //1. First Issue Was Created
        var issue1Id = Guid.NewGuid();
        EventStore.Append(streamId, new IssueCreated(issue1Id, "Description"));
        await Session.SaveChangesAsync();

        var issuesList = await EventStore.AggregateStreamAsync<IssuesList>(streamId);

        issuesList!.Issues.Should().HaveCount(1);
        issuesList.Issues.Values.Single().IssueId.Should().Be(issue1Id);
        issuesList.Issues.Values.Single().Description.Should().Be("Description");

        //2. First Issue Description Was Changed
        EventStore.Append(streamId, new IssueUpdated(issue1Id, "New Description"));
        await Session.SaveChangesAsync();

        issuesList = await EventStore.AggregateStreamAsync<IssuesList>(streamId);

        issuesList!.Issues.Should().HaveCount(1);
        issuesList.Issues.Values.Single().IssueId.Should().Be(issue1Id);
        issuesList.Issues.Values.Single().Description.Should().Be("New Description");

        //3. Two Other tasks were added
        EventStore.Append(streamId, new IssueCreated(Guid.NewGuid(), "Description2"),
            new IssueCreated(Guid.NewGuid(), "Description3"));
        await Session.SaveChangesAsync();

        issuesList = await EventStore.AggregateStreamAsync<IssuesList>(streamId);

        issuesList!.Issues.Should().HaveCount(3);
        issuesList.Issues.Values.Select(t => t.Description)
            .Should()
            .BeEquivalentTo("New Description", "Description2", "Description3");

        //4. First issue was removed
        EventStore.Append(streamId, new IssueRemoved(issue1Id));
        await Session.SaveChangesAsync();

        issuesList = await EventStore.AggregateStreamAsync<IssuesList>(streamId);

        issuesList!.Issues.Should().HaveCount(2);
        issuesList.Issues.Values.Select(t => t.Description)
            .Should()
            .BeEquivalentTo("Description2", "Description3");
    }
}
