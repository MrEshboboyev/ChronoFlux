using Core.Commands;

namespace Orders.Payments.RequestingPayment;

public record RequestPayment(
    Guid OrderId,
    decimal Amount
)
{
    public static RequestPayment Create(Guid orderId, decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(orderId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        return new RequestPayment(orderId, amount);
    }
}

public class HandleRequestPayment(PaymentsApiClient client): ICommandHandler<RequestPayment>
{
    public async Task HandleAsync(RequestPayment command, CancellationToken ct)
    {
       var result =  await client.Request(command, ct);

       result.EnsureSuccessStatusCode();
    }
}
