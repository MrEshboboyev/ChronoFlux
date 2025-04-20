namespace Tickets.Reservations.CreatingTentativeReservation;

public record TentativeReservationCreated(
    Guid ReservationId,
    Guid SeatId,
    string Number
)
{
    public static TentativeReservationCreated Create(Guid reservationId, Guid seatId, string number)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(reservationId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(seatId, Guid.Empty);
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentOutOfRangeException(nameof(number));

        return new TentativeReservationCreated(reservationId, seatId, number);
    }
}
