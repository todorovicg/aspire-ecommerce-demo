using Ecommerce.ApiDefaults;
using Ecommerce.Catalog.Api.Features;
using Ecommerce.Catalog.Api.Features.GetProduct;
using Ecommerce.Catalog.Api.Features.ListProducts;
using Ecommerce.Catalog.Api.Infrastructure;
using JasperFx;
using Microsoft.AspNetCore.Localization;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiDefaults();
builder.Services.AddValidation();
// Wolverine opens an explicit transaction around each handler, which EF Core forbids under the retrying execution strategy
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb", settings => settings.DisableRetry = true);
builder.Services.AddHostedService<CatalogDbInitializer>();
// Product text follows the Accept-Language header; anything outside the supported set falls back to English
builder.Services.AddRequestLocalization(options =>
{
    options.SetDefaultCulture(RequestLanguage.Default)
        .AddSupportedCultures(RequestLanguage.Default, "sr", "sr-Latn")
        .AddSupportedUICultures(RequestLanguage.Default, "sr", "sr-Latn");
    options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
});

builder.UseWolverine(options =>
{
    // Incoming OrderPaid events are tracked in a durable inbox in the catalog database, so a redelivery never decrements twice
    options.PersistMessagesWithPostgresql(builder.Configuration.GetConnectionString("catalogdb")!, "wolverine");
    options.UseEntityFrameworkCoreTransactions();
    options.Policies.AutoApplyTransactions();
    options.Policies.UseDurableInboxOnAllListeners();
    options.UseRabbitMqUsingNamedConnection("messaging")
        .AutoProvision()
        .DeclareExchange("order-paid", exchange => exchange.BindQueue("catalog.order-paid"));
    options.ListenToRabbitQueue("catalog.order-paid");
});

var app = builder.Build();

app.UseApiDefaults();
app.UseRequestLocalization();

var catalog = app.MapGroup("/api/catalog").WithTags("Catalog");
catalog.MapListProducts();
catalog.MapGetProduct();

return await app.RunJasperFxCommands(args);
