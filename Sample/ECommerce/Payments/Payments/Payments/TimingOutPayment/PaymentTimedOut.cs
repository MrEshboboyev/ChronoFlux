namespace Payments.Payments.TimingOutPayment;

public record PaymentTimedOut(
    Guid PaymentId,
    DateTime TimedOutAt
)
{
    public static PaymentTimedOut Create(Guid paymentId, in DateTime timedOutAt)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(paymentId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(timedOutAt, default);

        return new PaymentTimedOut(paymentId, timedOutAt);
    }
}
