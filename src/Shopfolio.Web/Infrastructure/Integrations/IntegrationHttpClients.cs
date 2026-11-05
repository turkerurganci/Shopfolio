namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The named HTTP clients of the outside services that go over HTTP (09 11.2 item 3): one per service. <c>HttpClient.Timeout</c> is infinite -
/// the limit is set per call by <see cref="ExternalCallRunner"/> -; the handler follows no redirect and keeps no cookie; no retry or resilience
/// handler is added (08 3.1 rule 2: the client library's own retry is off); the factory's own request loggers are removed, so no request line,
/// address or header reaches a log (09 11.2 item 5). The base address and the fixed headers are the implementing task's; a secret is given per
/// call, never to the client.
/// </summary>
internal static class IntegrationHttpClients
{
    public const string PaymentProvider = "PaymentProvider";

    public const string GoogleIdentity = "GoogleIdentity";

    public static readonly IReadOnlyList<string> Names = [PaymentProvider, GoogleIdentity];

    public static IServiceCollection AddIntegrationHttpClients(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        foreach (string name in Names)
        {
            services.AddHttpClient(name)
                .ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan)
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false, UseCookies = false })
                .RemoveAllLoggers();
        }

        return services;
    }
}
