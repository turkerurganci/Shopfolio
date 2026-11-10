using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>One request that reached the wire of a Google test: method, address, body and the content type.</summary>
internal sealed record GoogleRequest(string Method, Uri Uri, string Body, string? ContentType)
{
    public bool IsExchange => Method == "POST" && Uri.AbsoluteUri == GoogleEndpoints.Token;

    public bool IsDiscovery => Method == "GET" && Uri.AbsoluteUri == GoogleEndpoints.Discovery;

    public bool IsKeys => Method == "GET" && Uri.AbsoluteUri == GoogleTestSetup.JwksUri;

    /// <summary>The form body as name and value pairs, decoded.</summary>
    public Dictionary<string, string> Form() =>
        Body.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair => pair.Split('=', 2))
            .ToDictionary(p => Uri.UnescapeDataString(p[0].Replace('+', ' ')), p => Uri.UnescapeDataString(p[1].Replace('+', ' ')), StringComparer.Ordinal);
}

/// <summary>What a test makes the wire do for one request: answer with a status and a body, hang until the token is cancelled, or break the connection.</summary>
internal abstract record GoogleAnswer
{
    private GoogleAnswer()
    {
    }

    public sealed record Reply(HttpStatusCode Status, string Body) : GoogleAnswer;

    public sealed record Hang : GoogleAnswer;

    public sealed record Refuse : GoogleAnswer;

    public static GoogleAnswer Ok(string body) => new Reply(HttpStatusCode.OK, body);

    public static GoogleAnswer Exchanged(string idToken) => Ok("{\"access_token\":\"made-up-access-token-ACCESSTOKEN\",\"id_token\":" + JsonSerializer.Serialize(idToken) + ",\"token_type\":\"Bearer\"}");
}

/// <summary>The HTTP handler under the named Google client: it records every request and answers each by the test's script. No socket is opened.</summary>
internal sealed class GoogleWire : HttpMessageHandler
{
    private readonly Func<GoogleRequest, GoogleAnswer> _script;
    private readonly SemaphoreSlim _arrived = new(0);
    private readonly List<GoogleRequest> _requests = [];

    public GoogleWire(Func<GoogleRequest, GoogleAnswer> script)
    {
        _script = script;
    }

    public IReadOnlyList<GoogleRequest> Requests
    {
        get
        {
            lock (_requests)
            {
                return [.. _requests];
            }
        }
    }

    public Task NextRequestAsync(CancellationToken ct) => _arrived.WaitAsync(ct);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        GoogleRequest seen = new(request.Method.Method, request.RequestUri!, body, request.Content?.Headers.ContentType?.MediaType);
        lock (_requests)
        {
            _requests.Add(seen);
        }

        _arrived.Release();
        switch (_script(seen))
        {
            case GoogleAnswer.Reply reply:
                return new HttpResponseMessage(reply.Status) { Content = new StringContent(reply.Body, Encoding.UTF8, "application/json") };
            case GoogleAnswer.Hang:
                TaskCompletionSource never = new(TaskCreationOptions.RunContinuationsAsynchronously);
                await using (cancellationToken.Register(() => never.TrySetCanceled(cancellationToken)))
                {
                    await never.Task;
                }

                throw new UnreachableException();
            case GoogleAnswer.Refuse:
                throw new HttpRequestException(HttpRequestError.ConnectionError, "refused");
            default:
                throw new UnreachableException();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _arrived.Dispose();
        }

        base.Dispose(disposing);
    }
}

/// <summary>A signing key of the test's made-up Google: a real RSA key with a key id, never a key of any service.</summary>
internal sealed class GoogleTestKey : IDisposable
{
    private readonly RSA _rsa = RSA.Create(2048);

    public GoogleTestKey(string keyId)
    {
        KeyId = keyId;
    }

    public string KeyId { get; }

    public SigningCredentials Credentials(string algorithm = SecurityAlgorithms.RsaSha256) =>
        new(new RsaSecurityKey(_rsa) { KeyId = KeyId }, algorithm);

    /// <summary>The key's entry of the key set document.</summary>
    public string SetEntry()
    {
        RSAParameters parameters = _rsa.ExportParameters(includePrivateParameters: false);
        return "{\"kty\":\"RSA\",\"alg\":\"RS256\",\"use\":\"sig\",\"kid\":\"" + KeyId + "\",\"n\":\""
            + Base64UrlEncoder.Encode(parameters.Modulus!) + "\",\"e\":\"" + Base64UrlEncoder.Encode(parameters.Exponent!) + "\"}";
    }

    public void Dispose() => _rsa.Dispose();
}

/// <summary>The made-up installation values and documents the Google gateway tests share.</summary>
internal static class GoogleTestSetup
{
    public const string ClientId = "1234567890-madeupclientid.apps.googleusercontent.com";

    public const string ClientSecret = "made-up-google-client-secret-CLIENTSECRET";

    public const string Domain = "shop.example.com";

    public const string JwksUri = "https://www.googleapis.com/oauth2/v3/certs";

    public const string Issuer = "https://accounts.google.com";

    public const string SubjectValue = "109876543210123456789-SUBJECTID";

    public const string Nonce = "flow-nonce-value-NONCE";

    public static readonly IdentityCodeExchange Exchange = new("4/made-up-authorization-code-AUTHCODE", "made-up-proof-key-verifier-VERIFIER-0123456789");

    public static GoogleOptions Options() => new() { ClientId = ClientId, ClientSecret = ClientSecret };

    public static string DiscoveryDocument() =>
        "{\"issuer\":\"" + Issuer + "\",\"authorization_endpoint\":\"" + GoogleEndpoints.Authorization + "\",\"token_endpoint\":\"" + GoogleEndpoints.Token
        + "\",\"jwks_uri\":\"" + JwksUri + "\",\"id_token_signing_alg_values_supported\":[\"RS256\"]}";

    public static string KeySet(params GoogleTestKey[] keys) => "{\"keys\":[" + string.Join(",", keys.Select(k => k.SetEntry())) + "]}";

    /// <summary>A valid identity token's claims; a test overrides or removes what it wants to break.</summary>
    public static Dictionary<string, object> Claims(Action<Dictionary<string, object>>? change = null)
    {
        Dictionary<string, object> claims = new()
        {
            ["sub"] = SubjectValue,
            ["email"] = "ayse@example.com",
            ["email_verified"] = true,
            ["name"] = "Ayşe Yılmaz",
            ["hd"] = "example.com",
            ["nonce"] = Nonce,
            ["auth_time"] = DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeSeconds(),
            ["picture"] = "https://example.invalid/photo.jpg",
            ["given_name"] = "Ayşe",
        };
        change?.Invoke(claims);
        return claims;
    }

    public static string Token(GoogleTestKey key, Dictionary<string, object>? claims = null, string issuer = Issuer, string audience = ClientId, TimeSpan? expiresIn = null, string algorithm = SecurityAlgorithms.RsaSha256, bool signed = true)
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan life = expiresIn ?? TimeSpan.FromMinutes(10);
        SecurityTokenDescriptor descriptor = new()
        {
            Claims = claims ?? Claims(),
            Issuer = issuer,
            Audience = audience,
            IssuedAt = now.AddHours(-3),
            NotBefore = now.AddHours(-3),
            Expires = now.Add(life),
            SigningCredentials = signed ? key.Credentials(algorithm) : null,
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    /// <summary>A wire script that answers the exchange with <paramref name="idToken"/>, the discovery document and the key set.</summary>
    public static Func<GoogleRequest, GoogleAnswer> Script(string idToken, params GoogleTestKey[] keys) =>
        request => request switch
        {
            { IsExchange: true } => GoogleAnswer.Exchanged(idToken),
            { IsDiscovery: true } => GoogleAnswer.Ok(DiscoveryDocument()),
            { IsKeys: true } => GoogleAnswer.Ok(KeySet(keys)),
            _ => new GoogleAnswer.Reply(HttpStatusCode.NotFound, "{}"),
        };
}

/// <summary>
/// The gateway built the way the application builds it (the same registration, with the named client's handler replaced by the test's wire and the runner on a fake clock whose
/// log lines are kept in memory).
/// </summary>
internal sealed class GoogleRig : IDisposable
{
    private readonly ServiceProvider _provider;

    public GoogleRig(Func<GoogleRequest, GoogleAnswer> script, GoogleOptions? options = null)
    {
        Wire = new GoogleWire(script);
        Harness = new RunnerHarness();
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(Harness.Time);
        services.AddSingleton(Harness.Runner);
        services.AddIntegrationHttpClients();
        services.AddHttpClient(IntegrationHttpClients.GoogleIdentity).ConfigurePrimaryHttpMessageHandler(() => Wire);
        services.AddSingleton<IOptions<GoogleOptions>>(Options.Create(options ?? GoogleTestSetup.Options()));
        services.AddSingleton<IOptions<SiteOptions>>(Options.Create(new SiteOptions { Domain = GoogleTestSetup.Domain }));
        services.AddGoogleSignIn();
        _provider = services.BuildServiceProvider();
        Gateway = _provider.GetRequiredService<IGoogleIdentityGateway>();
    }

    public GoogleWire Wire { get; }

    public RunnerHarness Harness { get; }

    public IGoogleIdentityGateway Gateway { get; }

    public IServiceProvider Services => _provider;

    public Task<GoogleIdentityResult> VerifyAsync(CancellationToken ct) => Gateway.VerifyIdentityAsync(GoogleTestSetup.Exchange, ct);

    public void Dispose()
    {
        _provider.Dispose();
        Harness.Dispose();
        Wire.Dispose();
    }
}
