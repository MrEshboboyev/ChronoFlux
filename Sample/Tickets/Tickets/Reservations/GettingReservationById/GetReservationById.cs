using Core.Exceptions;
using Core.Queries;
using Marten;

namespace Tickets.Reservations.GettingReservationById;

public record GetReservationById(
    Guid ReservationId
);


internal class HandleGetReservationById(IDocumentSession querySession):
    IQueryHandler<GetReservationById, ReservationDetails>
{
    public async Task<ReservationDetails> HandleAsync(GetReservationById request, CancellationToken cancellationToken) =>
        await querySession.LoadAsync<ReservationDetails>(request.ReservationId, cancellationToken)
        ?? throw AggregateNotFoundException.For<ReservationDetails>(request.ReservationId);
}
