using Ecommerce.ApiDefaults;
using Ecommerce.Payment.Api.Features;
using Ecommerce.Payment.Api.Features.Authorize;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiDefaults();
builder.Services.AddValidation();
builder.Services.AddOptions<PaymentOptions>().BindConfiguration(PaymentOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<PaymentService>();

var app = builder.Build();

app.UseApiDefaults();

var payments = app.MapGroup("/api/payments").WithTags("Payments");
payments.MapAuthorize();

app.Run();
