using Ecommerce.Basket.Api.Domain;

namespace Ecommerce.Basket.Api.Infrastructure;

public interface IBasketStore
{
    Task<ShoppingBasket?> GetAsync(Guid buyerId, CancellationToken cancellationToken);

    Task SaveAsync(ShoppingBasket basket, CancellationToken cancellationToken);

    Task DeleteAsync(Guid buyerId, CancellationToken cancellationToken);
}
