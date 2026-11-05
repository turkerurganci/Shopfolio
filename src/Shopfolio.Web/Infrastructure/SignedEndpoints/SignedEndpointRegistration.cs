namespace Shopfolio.Infrastructure.SignedEndpoints;

/// <summary>
/// Registers the common pipeline of the signed machine endpoints and maps one such endpoint (08 3.2; 07 4.30.1). The endpoint's own part
/// (<see cref="ISignedEndpoint{TNotice}"/>) is registered and mapped by the task that builds it; today that is the payment provider's
/// notification (UC-80).
/// </summary>
internal static class SignedEndpointRegistration
{
    public static IServiceCollection AddSignedEndpoints(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<SignatureFailureLog>();
        services.AddSingleton<SignedEndpointHandler>();
        return services;
    }

    /// <summary>
    /// Maps <paramref name="path"/> as a signed machine endpoint: POST only, handled by the common pipeline with the request scope's
    /// <typeparamref name="TEndpoint"/> and transaction runner, and outside the protection against cross-site forgery (07 2.5.5; 05 6.5.1).
    /// </summary>
    public static IEndpointConventionBuilder MapSignedEndpoint<TEndpoint, TNotice>(this IEndpointRouteBuilder endpoints, string path)
        where TEndpoint : ISignedEndpoint<TNotice>
        where TNotice : class
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrEmpty(path);
        return endpoints
            .MapPost(path, context => context.RequestServices.GetRequiredService<SignedEndpointHandler>().HandleAsync(
                context,
                context.RequestServices.GetRequiredService<TEndpoint>(),
                context.RequestServices.GetRequiredService<ITransactionRunner>()))
            .DisableAntiforgery();
    }
}
