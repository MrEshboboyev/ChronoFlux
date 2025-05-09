using Carts.ShoppingCarts;
using Carts.ShoppingCarts.OpeningCart;
using Carts.Tests.Extensions.Reservations;
using Carts.Tests.Stubs.Repositories;
using FluentAssertions;
using Xunit;

namespace Carts.Tests.Carts.InitializingCart;

public class OpenShoppingCardCommandHandlerTests
{
    [Fact]
    public async Task ForInitCardCommand_ShouldAddNewCart()
    {
        // Given
        var repository = new FakeRepository<ShoppingCart>();

        var commandHandler = new HandleOpenShoppingCart(repository);

        var command = OpenShoppingCart.Create(Guid.NewGuid(), Guid.NewGuid());

        // When
        await commandHandler.HandleAsync(command, CancellationToken.None);

        //Then
        repository.Aggregates.Should().HaveCount(1);

        var cart = repository.Aggregates.Values.Single();

        cart
            .IsOpenedCartWith(
                command.CartId,
                command.ClientId
            )
            .HasCartOpenedEventWith(
                command.CartId,
                command.ClientId
            );
    }
}
