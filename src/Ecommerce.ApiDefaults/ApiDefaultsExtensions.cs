using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Ecommerce.ApiDefaults;

public static class ApiDefaultsExtensions
{
    // The envelope writer must be registered before AddProblemDetails so it is asked first
    public static TBuilder AddApiErrorHandling<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IProblemDetailsWriter, ApiEnvelopeProblemDetailsWriter>());
        builder.Services.AddProblemDetails();

        return builder;
    }

    public static TBuilder AddApiDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.AddApiErrorHandling();
        builder.Services.AddOpenApi();

        return builder;
    }

    public static WebApplication UseApiErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            StatusCodeSelector = exception => exception is BadHttpRequestException badRequest
                ? badRequest.StatusCode
                : StatusCodes.Status500InternalServerError,
        });
        app.UseStatusCodePages();

        return app;
    }

    public static WebApplication UseApiDefaults(this WebApplication app)
    {
        app.UseApiErrorHandling();
        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }
}
