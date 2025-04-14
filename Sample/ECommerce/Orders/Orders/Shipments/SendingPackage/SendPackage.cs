using Core.Commands;
using Core.Requests;
using Orders.Products;

namespace Orders.Shipments.SendingPackage;

public record SendPackage(
    Guid OrderId,
    IReadOnlyList<ProductItem> ProductItems
)
{
    public static SendPackage Create(
        Guid orderId,
        IReadOnlyList<ProductItem> productItems
    )
    {
        ArgumentOutOfRangeException.ThrowIfEqual(orderId, Guid.Empty);
        if (productItems.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(productItems.Count));

        return new SendPackage(orderId, productItems);
    }
}

public class HandleSendPackage(ExternalServicesConfig externalServicesConfig, IExternalCommandBus externalCommandBus)
    :
        ICommandHandler<SendPackage>
{
    public async Task HandleAsync(SendPackage command, CancellationToken ct) =>
        await externalCommandBus.PostAsync(
            externalServicesConfig.ShipmentsUrl!,
            "shipments",
            command,
            ct);
}
