using System.ComponentModel.DataAnnotations;
using Ecommerce.ApiDefaults;
using Ecommerce.Preferences.Api.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Preferences.Api.Features.UpdatePreferences;

public sealed record UpdatePreferencesRequest([property: Required, StringLength(8)] string Language);

public static class UpdatePreferencesEndpoint
{
    public static RouteGroupBuilder MapUpdatePreferences(this RouteGroupBuilder group)
    {
        group.MapPut("/", HandleAsync).WithName("UpdatePreferences");
        return group;
    }

    public static async Task<Results<Ok<ApiResponse<PreferencesResponse>>, BadRequest<ApiResponse>>> HandleAsync(
        BuyerId buyerId,
        UpdatePreferencesRequest request,
        PreferencesService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(buyerId, request.Language, cancellationToken);

        return result switch
        {
            UpdatePreferencesResult.Updated updated => ApiResults.Ok(PreferencesResponse.From(updated.Preferences)),
            UpdatePreferencesResult.UnsupportedLanguage unsupported => ApiResults.BadRequest(ErrorCodes.UnsupportedLanguage, $"Language [{unsupported.Language}] is not supported", new { Supported = Languages.Supported }),
            _ => throw new InvalidOperationException($"Unhandled result [{result.GetType().Name}]"),
        };
    }
}
