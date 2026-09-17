using Ecommerce.ApiDefaults;
using Ecommerce.Contracts;
using Ecommerce.Ordering.Api.Features.GetOrder;
using Ecommerce.Ordering.Api.Features.ListOrders;
using Ecommerce.Ordering.Api.Infrastructure;
using Ecommerce.Ordering.Api.Processing;
using JasperFx;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

MongoSerialization.Register();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiDefaults();
builder.Services.AddValidation();
builder.AddMongoDBClient("orderingdb");
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IOrderStore, MongoOrderStore>();
builder.Services.AddHostedService<OrderingDbInitializer>();
builder.Services.AddOptions<OrderProcessorOptions>().BindConfiguration(OrderProcessorOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(client => client.BaseAddress = new Uri("https+http://payment-api"));
builder.Services.AddScoped<IOrderPaidPublisher, WolverineOrderPaidPublisher>();
builder.Services.AddScoped<OrderProcessingService>();
builder.Services.AddHostedService<OrderProcessor>();

builder.UseWolverine(options =>
{
    // Outgoing OrderPaid events are stored in the orderingmessages database before delivery; orders themselves stay in Mongo
    options.PersistMessagesWithPostgresql(builder.Configuration.GetConnectionString("orderingmessages")!, "wolverine");
    options.UseRabbitMqUsingNamedConnection("messaging")
        .AutoProvision()
        .DeclareExchange("order-placed", exchange => exchange.BindQueue("ordering.order-placed"));
    options.ListenToRabbitQueue("ordering.order-placed").ProcessInline();
    options.PublishMessage<OrderPaid>().ToRabbitExchange("order-paid").UseDurableOutbox();
});

var app = builder.Build();

app.UseApiDefaults();

var orders = app.MapGroup("/api/orders").WithTags("Orders").AddEndpointFilter<BuyerIdFilter>();
orders.MapListOrders();
orders.MapGetOrder();

return await app.RunJasperFxCommands(args);
