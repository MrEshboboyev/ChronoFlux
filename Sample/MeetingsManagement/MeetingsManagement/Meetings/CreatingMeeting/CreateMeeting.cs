using Core.Commands;
using Core.Marten.Repository;

namespace MeetingsManagement.Meetings.CreatingMeeting;

public record CreateMeeting(
    Guid Id,
    string Name
);

internal class HandleCreateMeeting(IMartenRepository<Meeting> repository):
    ICommandHandler<CreateMeeting>
{
    public Task HandleAsync(CreateMeeting command, CancellationToken ct)
    {
        var (id, name) = command;

        return repository.Add(
            id,
            Meeting.New(id, name),
            ct
        );
    }
}
