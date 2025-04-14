using Core.Commands;
using Core.Marten.Repository;

namespace Carts.ShoppingCarts.CancelingCart;

public record CancelShoppingCart(
    Guid CartId
)
{
    public static CancelShoppingCart Create(Guid cartId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(cartId, Guid.Empty);

        return new CancelShoppingCart(cartId);
    }
}

internal class HandleCancelShoppingCart(IMartenRepository<ShoppingCart> cartRepository):
    ICommandHandler<CancelShoppingCart>
{
    public Task HandleAsync(CancelShoppingCart command, CancellationToken ct) =>
        cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.Cancel(),
            ct: ct
        );
}
