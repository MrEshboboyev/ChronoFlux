using Core.Commands;
using Core.Marten.Repository;

namespace Carts.ShoppingCarts.ConfirmingCart;

public record ConfirmShoppingCart(
    Guid CartId
)
{
    public static ConfirmShoppingCart Create(Guid cartId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(cartId, Guid.Empty);

        return new ConfirmShoppingCart(cartId);
    }
}

internal class HandleConfirmShoppingCart(IMartenRepository<ShoppingCart> cartRepository):
    ICommandHandler<ConfirmShoppingCart>
{
    public Task HandleAsync(ConfirmShoppingCart command, CancellationToken ct) =>
        cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.Confirm(),
            ct: ct
        );
}
