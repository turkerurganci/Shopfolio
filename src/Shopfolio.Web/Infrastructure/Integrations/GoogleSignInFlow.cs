using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>The two flows that go through Google (08 2.3; 07 UC-50, UC-62): sign-in and registration share one, the reauthentication is the other (T516's).</summary>
internal enum GoogleFlowKind
{
    SignInOrRegistration,

    Reauthentication,
}

/// <summary>The values the flow cookie holds (05 6.7.12): the flow's identifier, the nonce the identity token must carry back and the proof key's verifier. Used once.</summary>
internal sealed record GoogleFlowValues(string FlowId, string Nonce, string CodeVerifier);

/// <summary>What the protected <c>state</c> carries (09 11.3): the flow's identifier and kind, the start moment, the reauthentication's operation and the return path.</summary>
internal sealed record GoogleFlowState(string FlowId, GoogleFlowKind Kind, DateTimeOffset StartedAt, string? Operation, string? ReturnUrl);

/// <summary>A flow's consumed values: the cookie's and the state's, found to belong together and to be young enough.</summary>
internal sealed record GoogleFlow(GoogleFlowValues Values, GoogleFlowState State);

/// <summary>
/// The start and the end of a Google flow (T515; 05 6.1.4, 6.7.12; 07 UC-50, UC-51; 09 15.1 item 7). <see cref="Start"/> makes the flow's identifier, the nonce and the proof key's verifier from the
/// system's random source, writes them to the flow cookie (encrypted and signed, ten minutes) and returns the authorization address whose <c>state</c> is the identifier, the kind, the start moment and the
/// return path under the application's data protection key - the return path never travels in the clear. <see cref="Consume"/> reads the cookie and deletes it in the same response, then opens the
/// <c>state</c>: a missing or damaged cookie, a state that does not unprotect, an identifier that differs, or a start older than the cookie's life gives nothing, and the return has no flow
/// (a renewed or foreign return is a failed one). Nothing is stored on the server. The call reaches no network (08 2.3).
/// </summary>
internal sealed class GoogleSignInFlow
{
    private const char Separator = '|';

    private readonly BrowserCookies _cookies;
    private readonly ProtectedValues _protectedValues;
    private readonly IOptions<GoogleOptions> _google;
    private readonly IOptions<SiteOptions> _site;
    private readonly TimeProvider _time;
    private readonly IGoogleIdentityGateway _gateway;

    public GoogleSignInFlow(BrowserCookies cookies, ProtectedValues protectedValues, IOptions<GoogleOptions> google, IOptions<SiteOptions> site, TimeProvider time, IGoogleIdentityGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(cookies);
        ArgumentNullException.ThrowIfNull(protectedValues);
        ArgumentNullException.ThrowIfNull(google);
        ArgumentNullException.ThrowIfNull(site);
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(gateway);
        _cookies = cookies;
        _protectedValues = protectedValues;
        _google = google;
        _site = site;
        _time = time;
        _gateway = gateway;
    }

    /// <summary>
    /// Starts a flow: writes the flow cookie and returns Google's authorization address. A new start overwrites the cookie, so the earlier flow's return no longer holds (07 UC-50 idempotency). The caller
    /// has checked that the Google application is defined; an empty client identifier is a programming error here.
    /// </summary>
    public Uri Start(HttpContext context, GoogleFlowKind kind, string? operation, string? returnUrl)
    {
        ArgumentNullException.ThrowIfNull(context);
        string flowId = SecretKey.Create();
        string nonce = SecretKey.Create();
        string verifier = SecretKey.Create();
        DateTimeOffset startedAt = _time.GetUtcNow();
        string state = _protectedValues.Protect(
            DataProtectionPurposes.ExternalLoginState,
            string.Join(Separator, flowId, (int)kind, startedAt.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), operation ?? string.Empty, returnUrl ?? string.Empty));
        _cookies.WriteExternalLoginFlow(context, string.Join(Separator, flowId, nonce, verifier));
        return GoogleAuthorizationRequest.Build(_google.Value.ClientId, _site.Value.Domain, state, nonce, verifier, kind == GoogleFlowKind.Reauthentication);
    }

    /// <summary>The flow a return belongs to, or null when it belongs to none; the cookie is deleted in the same response either way (09 15.1 item 7).</summary>
    public GoogleFlow? Consume(HttpContext context, string? state)
    {
        ArgumentNullException.ThrowIfNull(context);
        ProtectedValueResult cookie = _cookies.ConsumeExternalLoginFlow(context);
        if (!cookie.IsValid || string.IsNullOrEmpty(state))
        {
            return null;
        }

        string[] held = cookie.Payload.Split(Separator);
        if (held.Length != 3 || held.Any(string.IsNullOrEmpty))
        {
            return null;
        }

        ProtectedValueResult opened = _protectedValues.Unprotect(DataProtectionPurposes.ExternalLoginState, state);
        if (!opened.IsValid)
        {
            return null;
        }

        // The return path comes last and may hold the separator itself.
        string[] parts = opened.Payload.Split(Separator, 5);
        if (parts.Length != 5
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int kindNumber)
            || !Enum.IsDefined((GoogleFlowKind)kindNumber)
            || !long.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out long startedSeconds)
            || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(parts[0]), Encoding.UTF8.GetBytes(held[0])))
        {
            return null;
        }

        DateTimeOffset startedAt = DateTimeOffset.FromUnixTimeSeconds(startedSeconds);
        TimeSpan age = _time.GetUtcNow() - startedAt;
        if (age < TimeSpan.Zero || age > BrowserCookies.ExternalLoginFlowLifetime)
        {
            return null;
        }

        return new GoogleFlow(
            new GoogleFlowValues(held[0], held[1], held[2]),
            new GoogleFlowState(parts[0], (GoogleFlowKind)kindNumber, startedAt, parts[3].Length == 0 ? null : parts[3], parts[4].Length == 0 ? null : parts[4]));
    }

    /// <summary>Whether the nonce the identity token carried is the one the flow made; the comparison takes the same time whatever the difference (09 15.1 item 7).</summary>
    public static bool NonceMatches(GoogleFlowValues flow, string? returned)
    {
        ArgumentNullException.ThrowIfNull(flow);
        return !string.IsNullOrEmpty(returned)
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(flow.Nonce), Encoding.UTF8.GetBytes(returned));
    }

    /// <summary>
    /// Verifies a return's authorization code with Google (the gateway's one call, outside any transaction, 09 11.3) and returns the identity when it is verified and carries the flow's nonce; an
    /// unreachable Google, an error answer, a failed verification and a nonce that is not the flow's are all the same ending, null (08 2.3; 05 10.1.5).
    /// </summary>
    public async Task<GoogleIdentityResult.Succeeded?> VerifyAsync(GoogleFlowValues flow, string code, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(flow);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        GoogleIdentityResult answer = await _gateway.VerifyIdentityAsync(new IdentityCodeExchange(code, flow.CodeVerifier), ct);
        return answer is GoogleIdentityResult.Succeeded identity && NonceMatches(flow, identity.Nonce) ? identity : null;
    }
}
