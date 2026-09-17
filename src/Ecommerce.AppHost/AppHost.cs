using JasperFx.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");
var basketDb = postgres.AddDatabase("basketdb");
var orderingMessagesDb = postgres.AddDatabase("orderingmessages");

var redis = builder.AddRedis("redis");

var rabbitMq = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin();

var mongo = builder.AddMongoDB("mongo");
var orderingDb = mongo.AddDatabase("orderingdb");

var catalogApi = builder.AddProject<Projects.Ecommerce_Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithHttpHealthCheck("/health")
    .WithJasperFxCommands();

var paymentApi = builder.AddProject<Projects.Ecommerce_Payment_Api>("payment-api")
    .WithHttpHealthCheck("/health");

var preferencesApi = builder.AddProject<Projects.Ecommerce_Preferences_Api>("preferences-api")
    .WithReference(redis)
    .WaitFor(redis)
    .WithHttpHealthCheck("/health");

var basketApi = builder.AddProject<Projects.Ecommerce_Basket_Api>("basket-api")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(basketDb)
    .WaitFor(basketDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithReference(catalogApi)
    .WaitFor(catalogApi)
    .WithHttpHealthCheck("/health")
    .WithJasperFxCommands();

var orderingApi = builder.AddProject<Projects.Ecommerce_Ordering_Api>("ordering-api")
    .WithReference(orderingDb)
    .WaitFor(orderingDb)
    .WithReference(orderingMessagesDb)
    .WaitFor(orderingMessagesDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithReference(paymentApi)
    .WithHttpHealthCheck("/health")
    .WithJasperFxCommands();

var frontend = builder.AddViteApp("frontend", "../ecommerce-frontend");

var gateway = builder.AddProject<Projects.Ecommerce_Gateway>("gateway")
    .WithReference(catalogApi)
    .WaitFor(catalogApi)
    .WithReference(basketApi)
    .WaitFor(basketApi)
    .WithReference(orderingApi)
    .WaitFor(orderingApi)
    .WithReference(preferencesApi)
    .WaitFor(preferencesApi)
    .WithReference(frontend)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

// The dev server proxies /api to the gateway, so the frontend URL in the dashboard works on its own
frontend.WithReference(gateway);

builder.Build().Run();
