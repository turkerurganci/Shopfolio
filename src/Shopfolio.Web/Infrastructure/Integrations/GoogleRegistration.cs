using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Registers Google's gateway (09 11.3 item 2): the one <see cref="ConfigurationManager{T}"/> of the discovery document - the fixed discovery address, the OpenID Connect
/// configuration retriever and a document retriever over the named Google client - and <see cref="GoogleIdentityGateway"/> as the application's
/// <see cref="IGoogleIdentityGateway"/>. The manager is made when it is first asked for and reads nothing then: no start of the application and no start of a flow reaches Google
/// (08 2.3). With no Google application in the settings the gateway is still registered and makes no call.
/// </summary>
internal static class GoogleRegistration
{
    public static IServiceCollection AddGoogleSignIn(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton(provider => new ConfigurationManager<OpenIdConnectConfiguration>(
            GoogleEndpoints.Discovery,
            new OpenIdConnectConfigurationRetriever(),
            new GoogleDocumentRetriever(provider.GetRequiredService<IHttpClientFactory>(), provider.GetRequiredService<TimeProvider>())));
        services.AddSingleton<IGoogleIdentityGateway, GoogleIdentityGateway>();
        return services;
    }
}

/// <summary>
/// Reads the discovery document and the key set over the named Google client (09 11.3 item 2): a <see cref="HttpDocumentRetriever"/> that requires HTTPS, over a client the
/// factory makes for each read, so the factory's handler rotation reaches a manager that lives as long as the application. Each read has the user-waiting limit of its own
/// (08 2.3: three calls in a row at the worst, 15 seconds each) and is not retried: the manager calls its retriever with no cancellation of its own (observed in T409), and the
/// client's timeout is infinite, so without this limit one hung read would hold the manager's refresh for good.
/// </summary>
internal sealed class GoogleDocumentRetriever : IDocumentRetriever
{
    private readonly IHttpClientFactory _clients;
    private readonly TimeProvider _time;

    public GoogleDocumentRetriever(IHttpClientFactory clients, TimeProvider time)
    {
        _clients = clients;
        _time = time;
    }

    public async Task<string> GetDocumentAsync(string address, CancellationToken cancel)
    {
        using var limit = new CancellationTokenSource(IntegrationOperations.LimitOf(IntegrationOperation.IdentityVerify), _time);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(limit.Token, cancel);
        return await new HttpDocumentRetriever(_clients.CreateClient(IntegrationHttpClients.GoogleIdentity)) { RequireHttps = true }.GetDocumentAsync(address, linked.Token);
    }
}
