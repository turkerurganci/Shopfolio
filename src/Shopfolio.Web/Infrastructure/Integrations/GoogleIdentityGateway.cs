using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using Shopfolio.Infrastructure.InstallationSettings;

using ValidationOutcome = Microsoft.IdentityModel.Tokens.TokenValidationResult;
using VerifiedJwt = Microsoft.IdentityModel.JsonWebTokens.JsonWebToken;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Google's implementation of <see cref="IGoogleIdentityGateway"/> (08 2.3; 09 11.1, 11.3): the authorization code flow's return - the code exchange and the identity
/// token's verification. The exchange is one POST to the fixed token address with the client secret in the body; <c>userinfo</c> is never called. The token is checked
/// by <see cref="JsonWebTokenHandler"/> against the key set the application's one <see cref="ConfigurationManager{T}"/> holds (cached; an unknown <c>kid</c> is the manager's
/// refresh, never a retry written here): only <c>RS256</c>, the signature, <c>iss</c> (the discovery document's issuer), <c>aud</c> (the client id) and <c>exp</c> (with the
/// 60 seconds of 09 3.5) are verified. Comparing <c>nonce</c> and <c>auth_time</c> is the core's (08 3.1 rule 4); the result carries them and never <c>sub</c>.
/// Each outside call runs through <see cref="ExternalCallRunner"/> with the user-waiting limit of its own and none is retried; a spent code is never sent again. A call
/// without both client settings is not made.
/// </summary>
internal sealed class GoogleIdentityGateway : IGoogleIdentityGateway
{
    private readonly IHttpClientFactory _clients;
    private readonly ExternalCallRunner _runner;
    private readonly IOptions<GoogleOptions> _google;
    private readonly IOptions<SiteOptions> _site;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _keys;
    private readonly JsonWebTokenHandler _handler = new();

    public GoogleIdentityGateway(
        IHttpClientFactory clients,
        ExternalCallRunner runner,
        IOptions<GoogleOptions> google,
        IOptions<SiteOptions> site,
        ConfigurationManager<OpenIdConnectConfiguration> keys)
    {
        _clients = clients;
        _runner = runner;
        _google = google;
        _site = site;
        _keys = keys;
    }

    public async Task<GoogleIdentityResult> VerifyIdentityAsync(IdentityCodeExchange exchange, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(exchange);
        GoogleOptions google = _google.Value;
        if (string.IsNullOrWhiteSpace(google.ClientId) || string.IsNullOrWhiteSpace(google.ClientSecret))
        {
            // 08 2.3: with either setting missing no call is made; the line is closed and the core never asks.
            return _runner.Failed<GoogleIdentityResult>(
                IntegrationOperation.IdentityVerify, ExternalCallFailure.Unrecognized(GoogleIdentityMapping.NotConfiguredReason));
        }

        Step<string> exchanged = await _runner.RunAsync(IntegrationOperation.IdentityVerify, token => ExchangeAsync(exchange, google, token), ct);
        if (exchanged is Step<string>.Ended exchangeEnded)
        {
            return exchangeEnded.Result;
        }

        string identityJwt = ((Step<string>.Done)exchanged).Value;
        Step<bool> keys = await _runner.RunAsync(IntegrationOperation.IdentityVerify, LoadKeysAsync, ct);
        if (keys is Step<bool>.Ended keysEnded)
        {
            return keysEnded.Result;
        }

        Step<JsonWebToken> validated = await _runner.RunAsync(IntegrationOperation.IdentityVerify, token => ValidateAsync(identityJwt, google.ClientId, token), ct);
        if (validated is Step<JsonWebToken>.Ended validationEnded)
        {
            return validationEnded.Result;
        }

        VerifiedJwt verified = ((Step<JsonWebToken>.Done)validated).Value;
        return Reported(GoogleIdentityMapping.ReadIdentity(Base64UrlEncoder.DecodeBytes(verified.EncodedPayload)));
    }

    /// <summary>
    /// The code exchange (08 2.3 "Giden çağrılar" item 1): <c>grant_type=authorization_code</c>, <c>code</c>, <c>client_id</c>, <c>client_secret</c>, <c>redirect_uri</c> and
    /// <c>code_verifier</c> as a form body to the fixed address.
    /// </summary>
    public static HttpRequestMessage ComposeExchange(IdentityCodeExchange exchange, GoogleOptions google, string domain)
    {
        ArgumentNullException.ThrowIfNull(exchange);
        ArgumentNullException.ThrowIfNull(google);
        return new HttpRequestMessage(HttpMethod.Post, GoogleEndpoints.Token)
        {
            Content = new FormUrlEncodedContent(
            [
                new("grant_type", "authorization_code"),
                new("code", exchange.AuthorizationCode),
                new("client_id", google.ClientId),
                new("client_secret", google.ClientSecret),
                new("redirect_uri", GoogleEndpoints.ReturnAddress(domain)),
                new("code_verifier", exchange.CodeVerifier),
            ]),
        };
    }

    /// <summary>The verification parameters (09 11.3 item 3): <c>RS256</c> only, a signature required, the manager's issuer, the client id as audience, the lifetime with its tolerance.</summary>
    public TokenValidationParameters ValidationParameters(string clientId)
    {
        return new TokenValidationParameters
        {
            ConfigurationManager = _keys,
            ValidAlgorithms = [GoogleEndpoints.SignatureAlgorithm],
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = clientId,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = GoogleEndpoints.IdentityTokenClockSkew,
        };
    }

    private GoogleIdentityResult Reported(GoogleIdentityResult read) =>
        read is GoogleIdentityResult.Failed failed
            ? _runner.Failed<GoogleIdentityResult>(IntegrationOperation.IdentityVerify, ExternalCallFailure.Unrecognized(failed.ReasonCode))
            : read;

    private async Task<Step<string>> ExchangeAsync(IdentityCodeExchange exchange, GoogleOptions google, CancellationToken token)
    {
        using HttpRequestMessage request = ComposeExchange(exchange, google, _site.Value.Domain);
        using HttpClient client = _clients.CreateClient(IntegrationHttpClients.GoogleIdentity);
        using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, token);
        string body = await response.Content.ReadAsStringAsync(token);
        GoogleExchangeOutcome outcome = GoogleIdentityMapping.ReadExchangeAnswer(response.StatusCode, body);
        return outcome.Failure is { } failure
            ? new Step<string>.Ended(_runner.Failed<GoogleIdentityResult>(IntegrationOperation.IdentityVerify, failure))
            : new Step<string>.Done(outcome.IdToken!); // non-null: ReadExchangeAnswer gives an id token whenever it gives no failure
    }

    /// <summary>Gets the discovery document and the key set into the manager's cache (or reads the cache) before the check; the call has the limit of its own and an outage is "unavailable".</summary>
    private async Task<Step<bool>> LoadKeysAsync(CancellationToken token)
    {
        try
        {
            _ = await _keys.GetConfigurationAsync(token).WaitAsync(token);
            return new Step<bool>.Done(true);
        }
        catch (InvalidOperationException) when (token.IsCancellationRequested)
        {
            // The manager wraps the cancellation of its own call; the limit that passed is the runner's to tell.
            throw new OperationCanceledException(token);
        }
        catch (InvalidOperationException)
        {
            return new Step<bool>.Ended(
                _runner.Failed<GoogleIdentityResult>(IntegrationOperation.IdentityVerify, ExternalCallFailure.Unreachable(GoogleIdentityMapping.ConfigurationReason)));
        }
    }

    private async Task<Step<JsonWebToken>> ValidateAsync(string identityJwt, string clientId, CancellationToken token)
    {
        // The handler's key refresh (an unknown kid) reaches the network with no token of ours: the wait is bounded here, so the user's wait is.
        ValidationOutcome result = await _handler.ValidateTokenAsync(identityJwt, ValidationParameters(clientId)).WaitAsync(token);
        if (result.IsValid && result.SecurityToken is JsonWebToken verified)
        {
            return new Step<JsonWebToken>.Done(verified);
        }

        ExternalCallFailure failure = GoogleIdentityMapping.IsConfigurationOutage(result.Exception)
            ? ExternalCallFailure.Unreachable(GoogleIdentityMapping.ConfigurationReason)
            : ExternalCallFailure.Unrecognized(GoogleIdentityMapping.ValidationReason(result.Exception));
        return new Step<JsonWebToken>.Ended(_runner.Failed<GoogleIdentityResult>(IntegrationOperation.IdentityVerify, failure));
    }

    /// <summary>One step of the verification: it is done with a value, or it ended the whole call with its result.</summary>
    private abstract record Step<T> : IExternalCallResult<Step<T>>
    {
        private Step()
        {
        }

        public sealed record Done(T Value) : Step<T>;

        public sealed record Ended(GoogleIdentityResult Result) : Step<T>;

        public static Step<T> FromFailure(ExternalCallFailure failure) => new Ended(GoogleIdentityResult.FromFailure(failure));
    }
}
