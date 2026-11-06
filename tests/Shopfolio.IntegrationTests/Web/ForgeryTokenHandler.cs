using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// Gives every writing request of a test client the two halves of the framework's forgery token, the way a browser that loaded the page would
/// (T209; 07 2.5.4): the handler asks the test-only token page (<see cref="TokenPath"/>) with the request's own cookies - so the token is made for
/// the same user - keeps the antiforgery cookie it answers with, and sets the cookie and the <c>RequestVerificationToken</c> header on the request.
/// A test that wants the refusal sets <see cref="SkipHeader"/> (no token at all) or the token header itself (a wrong or foreign value). The clients
/// of these tests handle no cookies themselves (the tests send their session cookies by hand), so the handler carries the antiforgery cookie too.
/// </summary>
public sealed class ForgeryTokenHandler : DelegatingHandler
{
    /// <summary>The test-only page that answers a token (<c>ForgeryTokenSampleModel</c>); it lives in the machine area of the address plan.</summary>
    public const string TokenPath = "/sistem/test-jeton";

    /// <summary>A request that carries this header is sent as it is, without a token; the header itself never reaches the server.</summary>
    public const string SkipHeader = "X-Test-No-Forgery-Token";

    /// <summary>The status of the answer when the token page itself could not be reached (a test setup mistake, not a product answer).</summary>
    private const string SetupMessage = "The forgery token page of the test host did not answer; the application part of the test assembly is missing.";

    public static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return factory.CreateDefaultClient(new Uri("https://localhost"), new ForgeryTokenHandler());
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Headers.Remove(SkipHeader) || !IsWriting(request.Method) || request.Headers.Contains(CookieSetup.AntiforgeryHeaderName))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        string scheme = request.RequestUri!.AbsolutePath.StartsWith("/panel", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Customer";
        using HttpRequestMessage probe = new(HttpMethod.Get, new Uri(request.RequestUri, $"{TokenPath}?scheme={scheme}"));
        string cookies = string.Join("; ", request.Headers.TryGetValues(HeaderNames.Cookie, out IEnumerable<string>? sent) ? sent : []);
        if (cookies.Length > 0)
        {
            probe.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookies);
        }

        using HttpResponseMessage answer = await base.SendAsync(probe, cancellationToken);
        if (!answer.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"{SetupMessage} Status {(int)answer.StatusCode}.");
        }

        string requestHalf = await answer.Content.ReadAsStringAsync(cancellationToken);
        string cookie = answer.Headers.GetValues(HeaderNames.SetCookie)
            .Select(c => c.Split(';', 2)[0])
            .First(c => c.StartsWith(CookieNames.Antiforgery + "=", StringComparison.Ordinal));

        request.Headers.Remove(HeaderNames.Cookie);
        request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookies.Length > 0 ? $"{cookies}; {cookie}" : cookie);
        request.Headers.TryAddWithoutValidation(CookieSetup.AntiforgeryHeaderName, requestHalf);
        return await base.SendAsync(request, cancellationToken);
    }

    private static bool IsWriting(HttpMethod method) =>
        method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Delete || method == HttpMethod.Patch;
}
