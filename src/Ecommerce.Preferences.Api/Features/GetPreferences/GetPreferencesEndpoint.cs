using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Preferences.Api.Features.GetPreferences;

public static class GetPreferencesEndpoint
{
    public static RouteGroupBuilder MapGetPreferences(this RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync).WithName("GetPreferences");
        return group;
    }

    public static async Task<Ok<ApiResponse<PreferencesResponse>>> HandleAsync(
        BuyerId buyerId,
        PreferencesService service,
        CancellationToken cancellationToken)
    {
        var preferences = await service.GetAsync(buyerId, cancellationToken);
        return ApiResults.Ok(PreferencesResponse.From(preferences));
    }
}
