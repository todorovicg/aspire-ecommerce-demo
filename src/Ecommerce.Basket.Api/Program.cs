using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Features;
using Ecommerce.Basket.Api.Features.Checkout;
using Ecommerce.Basket.Api.Features.GetBasket;
using Ecommerce.Basket.Api.Features.RemoveItem;
using Ecommerce.Basket.Api.Features.UpsertItem;
using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Contracts;
using JasperFx;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiDefaults();
builder.Services.AddValidation();
builder.AddRedisClient("redis");
builder.Services.AddSingleton<IBasketStore, RedisBasketStore>();
// The buyer's Accept-Language travels to Catalog on every call, so product names come back in the buyer's language
builder.Services.AddHeaderPropagation(options => options.Headers.Add("Accept-Language"));
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client => client.BaseAddress = new Uri("https+http://catalog-api"))
    .AddHeaderPropagation();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IOrderPlacedPublisher, WolverineOrderPlacedPublisher>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<CheckoutService>();

builder.UseWolverine(options =>
{
    // The outbox tables live in the basket's own database; a message is persisted there before PublishAsync returns
    options.PersistMessagesWithPostgresql(builder.Configuration.GetConnectionString("basketdb")!, "wolverine");
    options.UseRabbitMqUsingNamedConnection("messaging").AutoProvision();
    options.PublishMessage<OrderPlaced>().ToRabbitExchange("order-placed").UseDurableOutbox();
});

var app = builder.Build();

app.UseApiDefaults();
app.UseHeaderPropagation();

var basket = app.MapGroup("/api/basket").WithTags("Basket").AddEndpointFilter<BuyerIdFilter>();
basket.MapGetBasket();
basket.MapUpsertItem();
basket.MapRemoveItem();
basket.MapCheckout();

return await app.RunJasperFxCommands(args);
