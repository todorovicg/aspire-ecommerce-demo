using Ecommerce.Basket.Api.Domain;

namespace Ecommerce.Basket.Api.Features;

// A basket together with the product names Catalog returned for its lines in the request's language
public sealed record NamedBasket(ShoppingBasket Basket, IReadOnlyDictionary<Guid, string> ProductNames);
