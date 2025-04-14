namespace Carts.ShoppingCarts.OpeningCart;

public record ShoppingCartOpened(
    Guid CartId,
    Guid ClientId
)
{
    public static ShoppingCartOpened Create(Guid cartId, Guid clientId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(cartId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(clientId, Guid.Empty);

        return new ShoppingCartOpened(cartId, clientId);
    }
}
