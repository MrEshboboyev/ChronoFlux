using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.Marten.Repository;

namespace Carts.ShoppingCarts.RemovingProduct;

public record RemoveProduct(
    Guid CartId,
    PricedProductItem ProductItem
)
{
    public static RemoveProduct Create(Guid cartId, PricedProductItem productItem)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(cartId, Guid.Empty);

        return new RemoveProduct(cartId, productItem);
    }
}

internal class HandleRemoveProduct(IMartenRepository<ShoppingCart> cartRepository):
    ICommandHandler<RemoveProduct>
{
    public Task HandleAsync(RemoveProduct command, CancellationToken ct)
    {
        var (cartId, productItem) = command;

        return cartRepository.GetAndUpdate(
            cartId,
            cart => cart.RemoveProduct(productItem),
            ct: ct
        );
    }
}
