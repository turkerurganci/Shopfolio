namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The fixed addresses and values of the Google sign-in (08 2.3; 09 11.3 item 1, 3.5 table): the authorization and the code exchange addresses are constants and are never read
/// from the discovery document, whose values of 2026-10-29 they are (observed again on 2026-11-10, T409: DV-31). Only the discovery document's own address is read from the
/// outside, and from it only the issuer and the key set's address (<c>jwks_uri</c>).
/// </summary>
internal static class GoogleEndpoints
{
    /// <summary>Where the browser is sent to sign in (08 2.3 "Tarayıcı yönlendirmeleri").</summary>
    public const string Authorization = "https://accounts.google.com/o/oauth2/v2/auth";

    /// <summary>The code exchange, a POST (08 2.3 "Giden çağrılar" item 1).</summary>
    public const string Token = "https://oauth2.googleapis.com/token";

    /// <summary>The discovery document: the issuer and the key set's address come from it (08 2.3 "Giden çağrılar" item 2).</summary>
    public const string Discovery = "https://accounts.google.com/.well-known/openid-configuration";

    /// <summary>The path of the one return address of the three flows: <c>https://{domain}/giris/google</c> (08 2.3).</summary>
    public const string ReturnPath = "/giris/google";

    /// <summary>The scopes the request asks for and no other (08 2.3).</summary>
    public const string Scope = "openid email profile";

    /// <summary>The proof key method; Google offers <c>plain</c> and <c>S256</c> (DV-27).</summary>
    public const string CodeChallengeMethod = "S256";

    /// <summary>
    /// Whether the re-authentication request carries <c>max_age</c>. It does only if DV-26 turns out true (08 2.3 (2); K-968); DV-26 has not been observed with a real application,
    /// so the request does not carry it. A change of this value is a change of the first-connection observation, not a tuning.
    /// </summary>
    public static bool SendsMaxAge => false;

    /// <summary>The tolerance of the identity token's <c>exp</c> against the clock (09 3.5 table; K-1328 (4)); <c>auth_time</c> has none (K-1272).</summary>
    public static readonly TimeSpan IdentityTokenClockSkew = TimeSpan.FromSeconds(60);

    /// <summary>The only signature algorithm accepted (08 2.3 "Geri çağrım").</summary>
    public const string SignatureAlgorithm = "RS256";

    /// <summary>The return address of a domain (08 2.3: <c>redirect_uri</c>).</summary>
    public static string ReturnAddress(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        return "https://" + domain + ReturnPath;
    }
}
