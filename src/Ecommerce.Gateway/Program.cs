using Ecommerce.ApiDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiErrorHandling();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseApiErrorHandling();
app.MapDefaultEndpoints();

// Unknown API paths get an envelope 404 instead of falling through to the frontend catch-all
app.Map("/api/{**rest}", () => TypedResults.NotFound());
app.MapReverseProxy();

app.Run();
