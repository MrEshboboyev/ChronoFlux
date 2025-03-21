using Core.Commands;
using Core.Events;
using HotelManagement.Sagas.GuestStayAccounts;

namespace HotelManagement.Sagas.GroupCheckouts;

public class GroupCheckoutSaga(IAsyncCommandBus commandBus):
    IEventHandler<GroupCheckoutInitiated>,
    IEventHandler<GuestStayAccounts.GuestCheckedOut>,
    IEventHandler<GuestStayAccounts.GuestCheckoutFailed>
{
    public async Task HandleAsync(GroupCheckoutInitiated @event, CancellationToken ct)
    {
        foreach (var guestAccountId in @event.GuestStayIds)
        {
            await commandBus.ScheduleAsync(
                new CheckOutGuest(guestAccountId, @event.GroupCheckoutId),
                ct
            );
        }

        await commandBus.ScheduleAsync(
            new RecordGuestCheckoutsInitiation(
                @event.GroupCheckoutId,
                @event.GuestStayIds
            ),
            ct
        );
    }

    public Task HandleAsync(GuestStayAccounts.GuestCheckedOut @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Task.CompletedTask;

        return commandBus.ScheduleAsync(
            new RecordGuestCheckoutCompletion(
                @event.GroupCheckOutId.Value,
                @event.GuestStayId,
                @event.CheckedOutAt
            ),
            ct
        );
    }

    public Task HandleAsync(GuestStayAccounts.GuestCheckoutFailed @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Task.CompletedTask;

        return commandBus.ScheduleAsync(
            new RecordGuestCheckoutFailure(
                @event.GroupCheckOutId.Value,
                @event.GuestStayId,
                @event.FailedAt
            ),
            ct
        );
    }
}
