using System.Security.Cryptography;
using System.Text;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The authorization request that starts a Google flow (08 2.3 "Tarayıcı yönlendirmeleri"; 09 11.3 item 1): a fixed address with <c>client_id</c>, <c>redirect_uri</c>,
/// <c>response_type=code</c>, <c>scope=openid email profile</c>, <c>state</c>, <c>nonce</c>, the proof key's challenge and its method <c>S256</c>. Re-authentication adds the
/// <c>claims</c> parameter that asks for <c>auth_time</c> under <c>id_token</c>; it carries <c>max_age</c> only while DV-26 is true (<see cref="GoogleEndpoints.SendsMaxAge"/>).
/// <c>response_mode</c>, <c>prompt</c>, <c>hd</c> and <c>login_hint</c> are never carried. The call makes no network call: the start of a flow does not reach Google (08 2.3).
/// </summary>
internal static class GoogleAuthorizationRequest
{
    /// <summary>The <c>claims</c> value of the re-authentication: <c>auth_time</c> is asked of the identity token and is essential (08 2.3 "Doğrulanacaklar" (1)).</summary>
    public const string AuthTimeClaims = """{"id_token":{"auth_time":{"essential":true}}}""";

    /// <summary>The <c>max_age</c> value of a request that carries it: a sign-in no older than "now" (08 2.3 (2)).</summary>
    public const string MaxAgeValue = "0";

    public static Uri Build(string clientId, string domain, string state, string nonce, string codeVerifier, bool reauthentication) =>
        Build(clientId, domain, state, nonce, codeVerifier, reauthentication, GoogleEndpoints.SendsMaxAge);

    /// <summary>The same request with the <c>max_age</c> decision given: the single place the decision is taken, so a test shows both outcomes.</summary>
    public static Uri Build(string clientId, string domain, string state, string nonce, string codeVerifier, bool reauthentication, bool sendMaxAge)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(nonce);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeVerifier);

        List<KeyValuePair<string, string>> parameters =
        [
            new("client_id", clientId),
            new("redirect_uri", GoogleEndpoints.ReturnAddress(domain)),
            new("response_type", "code"),
            new("scope", GoogleEndpoints.Scope),
            new("state", state),
            new("nonce", nonce),
            new("code_challenge", CodeChallenge(codeVerifier)),
            new("code_challenge_method", GoogleEndpoints.CodeChallengeMethod),
        ];

        if (reauthentication)
        {
            parameters.Add(new("claims", AuthTimeClaims));
            if (sendMaxAge)
            {
                parameters.Add(new("max_age", MaxAgeValue));
            }
        }

        StringBuilder query = new();
        foreach (KeyValuePair<string, string> parameter in parameters)
        {
            query.Append(query.Length == 0 ? '?' : '&').Append(parameter.Key).Append('=').Append(Uri.EscapeDataString(parameter.Value));
        }

        return new Uri(GoogleEndpoints.Authorization + query, UriKind.Absolute);
    }

    /// <summary>The S256 challenge of a verifier: the unpadded base64url of its SHA-256 hash (RFC 7636 4.2).</summary>
    public static string CodeChallenge(string codeVerifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codeVerifier);
        byte[] hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
