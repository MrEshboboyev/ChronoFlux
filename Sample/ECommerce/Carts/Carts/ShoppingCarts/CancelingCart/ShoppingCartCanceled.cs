namespace Carts.ShoppingCarts.CancelingCart;

public record ShoppingCartCanceled(
    Guid CartId,
    DateTime CanceledAt
)
{
    public static ShoppingCartCanceled Create(Guid cartId, DateTime canceledAt)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(cartId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(canceledAt, default);

        return new ShoppingCartCanceled(cartId, canceledAt);
    }
}
