using Ecommerce.ApiDefaults;
using Ecommerce.Preferences.Api.Features;
using Ecommerce.Preferences.Api.Features.GetPreferences;
using Ecommerce.Preferences.Api.Features.UpdatePreferences;
using Ecommerce.Preferences.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiDefaults();
builder.Services.AddValidation();
builder.AddRedisClient("redis");
builder.Services.AddSingleton<IPreferencesStore, RedisPreferencesStore>();
builder.Services.AddScoped<PreferencesService>();

var app = builder.Build();

app.UseApiDefaults();

var preferences = app.MapGroup("/api/preferences").WithTags("Preferences").AddEndpointFilter<BuyerIdFilter>();
preferences.MapGetPreferences();
preferences.MapUpdatePreferences();

app.Run();
