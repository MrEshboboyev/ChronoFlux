namespace Payments.Payments.RequestingPayment;

public record PaymentRequested(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount
)
{
    public static PaymentRequested Create(Guid paymentId, Guid orderId, in decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(paymentId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(orderId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        return new PaymentRequested(paymentId, orderId, amount);
    }
}
