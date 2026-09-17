namespace Ecommerce.Basket.Api;

public static class ErrorCodes
{
    public const string ProductNotFound = "basket.product_not_found";
    public const string ProductOutOfStock = "basket.product_out_of_stock";
    public const string CatalogUnavailable = "basket.catalog_unavailable";
    public const string CheckoutEmpty = "basket.checkout_empty";
    public const string CheckoutOutOfStock = "basket.checkout_out_of_stock";
    public const string OutboxUnavailable = "basket.outbox_unavailable";
}
