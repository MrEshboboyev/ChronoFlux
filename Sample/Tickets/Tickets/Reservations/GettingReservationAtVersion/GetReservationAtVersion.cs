using Core.Exceptions;
using Core.Queries;
using Marten;
using Tickets.Reservations.GettingReservationById;

namespace Tickets.Reservations.GettingReservationAtVersion;

public record GetReservationAtVersion(
    Guid ReservationId,
    int Version
)
{
    public static GetReservationAtVersion Create(Guid reservationId, int version)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(reservationId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegative(version);

        return new GetReservationAtVersion(reservationId, version);
    }
}

internal class HandleGetReservationAtVersion(IDocumentSession querySession):
    IQueryHandler<GetReservationAtVersion, ReservationDetails>
{
    public async Task<ReservationDetails> HandleAsync(GetReservationAtVersion query, CancellationToken cancellationToken)
    {
        var (reservationId, version) = query;
        return await querySession.Events.AggregateStreamAsync<ReservationDetails>(
            reservationId,
            version,
            token: cancellationToken
        ) ?? throw AggregateNotFoundException.For<ReservationDetails>(reservationId);
    }
}
