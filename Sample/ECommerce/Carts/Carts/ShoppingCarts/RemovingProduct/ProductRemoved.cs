using Carts.ShoppingCarts.Products;

namespace Carts.ShoppingCarts.RemovingProduct;

public record ProductRemoved(
    Guid CartId,
    PricedProductItem ProductItem
)
{
    public static ProductRemoved Create(Guid cartId, PricedProductItem productItem)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(cartId, Guid.Empty);

        return new ProductRemoved(cartId, productItem);
    }
}
