using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The named HTTP clients of the outside services (T202 criteria 3 and 4, KK3, KK4; 08 3.1 rule 2; 09 11.2 items 3 and 5): the handler chain
/// is the factory's lifetime handler straight over the socket handler - no retry, resilience or logging handler in between -, the client's own
/// timeout is infinite, redirects and cookies are off, and a refused connection through the chain goes once. No socket is opened.
/// </summary>
public sealed class IntegrationHttpClientsTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<string> Names => [.. IntegrationHttpClients.Names];

    private static ServiceProvider Provider(Action<IServiceCollection>? more = null)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddIntegrationHttpClients();
        more?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Theory]
    [MemberData(nameof(Names))]
    public void Handler_IsTheLifetimeHandlerOverTheSocketHandler_WithNoOtherHandler_NoRedirectAndNoCookie(string name)
    {
        using ServiceProvider provider = Provider();

        HttpMessageHandler outer = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(name);

        DelegatingHandler lifetime = Assert.IsAssignableFrom<DelegatingHandler>(outer);
        Assert.Equal("LifetimeTrackingHttpMessageHandler", lifetime.GetType().Name);
        SocketsHttpHandler primary = Assert.IsType<SocketsHttpHandler>(lifetime.InnerHandler);
        Assert.False(primary.AllowAutoRedirect);
        Assert.False(primary.UseCookies);
    }

    [Theory]
    [MemberData(nameof(Names))]
    public void Client_HasAnInfiniteTimeout_SoTheLimitIsTheCallsOwn(string name)
    {
        using ServiceProvider provider = Provider();

        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(name);

        Assert.Equal(Timeout.InfiniteTimeSpan, client.Timeout);
    }

    [Theory]
    [MemberData(nameof(Names))]
    public async Task RefusedConnection_ThroughTheChain_GoesOnce_AndIsTheFamilysConnectionClass(string name)
    {
        ScriptedHandler refusing = new() { Mode = ScriptedHandler.Behavior.Refuse };
        using ServiceProvider provider = Provider(services => services.AddHttpClient(name).ConfigurePrimaryHttpMessageHandler(() => refusing));
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(name);
        using RunnerHarness harness = new();

        PaymentQueryResult result = await harness.Runner.RunAsync(
            IntegrationOperation.PaymentQuery,
            async token =>
            {
                using HttpResponseMessage response = await client.GetAsync(new Uri("https://service.invalid/query"), token);
                return (PaymentQueryResult)new PaymentQueryResult.NoPayment();
            },
            Ct);

        Assert.Equal(new PaymentQueryResult.NoResponse(ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError)), result);
        Assert.Equal(1, refusing.Requests);
    }
}
