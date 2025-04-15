namespace Payments.Payments.DiscardingPayment;

public record PaymentDiscarded(
    Guid PaymentId,
    DiscardReason DiscardReason,
    DateTime DiscardedAt)
{
    public static PaymentDiscarded Create(Guid paymentId, DiscardReason discardReason, DateTime discardedAt)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(paymentId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(discardedAt, default);

        return new PaymentDiscarded(paymentId, discardReason, discardedAt);
    }
}
