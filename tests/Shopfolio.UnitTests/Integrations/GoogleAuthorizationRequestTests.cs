using System.Text.Json;
using System.Web;

using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The request that starts a Google flow (T409 criterion 3, KK3; 08 2.3 "Tarayıcı yönlendirmeleri"; K-968): the fixed authorization address; the eight parameters of every flow; the
/// <c>claims</c> parameter that asks <c>auth_time</c> under <c>id_token</c> on re-authentication only; no <c>prompt</c>, <c>response_mode</c>, <c>hd</c> or <c>login_hint</c> ever;
/// <c>max_age</c> only while DV-26 is true (it is not). Building the request calls nothing.
/// </summary>
public sealed class GoogleAuthorizationRequestTests
{
    private const string State = "flow-state-value-STATE";

    private const string Verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

    private static Uri Build(bool reauthentication, bool? sendMaxAge = null) =>
        sendMaxAge is null
            ? GoogleAuthorizationRequest.Build(GoogleTestSetup.ClientId, GoogleTestSetup.Domain, State, GoogleTestSetup.Nonce, Verifier, reauthentication)
            : GoogleAuthorizationRequest.Build(GoogleTestSetup.ClientId, GoogleTestSetup.Domain, State, GoogleTestSetup.Nonce, Verifier, reauthentication, sendMaxAge.Value);

    private static List<KeyValuePair<string, string>> Parameters(Uri uri)
    {
        System.Collections.Specialized.NameValueCollection collection = HttpUtility.ParseQueryString(uri.Query);
        return [.. collection.AllKeys.Select(k => new KeyValuePair<string, string>(k!, collection[k]!))];
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheAddress_IsTheFixedAuthorizationAddress_WithHttpsAndNoFragment(bool reauthentication)
    {
        Uri uri = Build(reauthentication);

        Assert.Equal("https://accounts.google.com/o/oauth2/v2/auth", uri.GetLeftPart(UriPartial.Path));
        Assert.Equal(string.Empty, uri.Fragment);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryFlow_CarriesTheEightParameters_WithTheirValues(bool reauthentication)
    {
        Dictionary<string, string> parameters = Parameters(Build(reauthentication)).ToDictionary(p => p.Key, p => p.Value);

        Assert.Equal(GoogleTestSetup.ClientId, parameters["client_id"]);
        Assert.Equal("https://shop.example.com/giris/google", parameters["redirect_uri"]);
        Assert.Equal("code", parameters["response_type"]);
        Assert.Equal("openid email profile", parameters["scope"]);
        Assert.Equal(State, parameters["state"]);
        Assert.Equal(GoogleTestSetup.Nonce, parameters["nonce"]);
        Assert.Equal(GoogleAuthorizationRequest.CodeChallenge(Verifier), parameters["code_challenge"]);
        Assert.Equal("S256", parameters["code_challenge_method"]);
    }

    [Fact]
    public void SignInAndSignUp_CarryNoClaimsParameter_AndNothingBeyondTheEight()
    {
        List<KeyValuePair<string, string>> parameters = Parameters(Build(reauthentication: false));

        Assert.Equal(8, parameters.Count);
        Assert.DoesNotContain(parameters, p => p.Key == "claims");
    }

    [Fact]
    public void Reauthentication_AsksAuthTimeUnderIdToken_ThroughTheClaimsParameter()
    {
        List<KeyValuePair<string, string>> parameters = Parameters(Build(reauthentication: true));

        string claims = Assert.Single(parameters, p => p.Key == "claims").Value;
        using JsonDocument document = JsonDocument.Parse(claims);
        JsonElement authTime = document.RootElement.GetProperty("id_token").GetProperty("auth_time");
        Assert.True(authTime.GetProperty("essential").GetBoolean());
        Assert.Single(document.RootElement.EnumerateObject());
        Assert.Single(document.RootElement.GetProperty("id_token").EnumerateObject());
        Assert.Equal(9, parameters.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoFlow_CarriesAForbiddenParameter(bool reauthentication)
    {
        string[] names = [.. Parameters(Build(reauthentication)).Select(p => p.Key)];

        string[] forbidden = ["prompt", "response_mode", "hd", "login_hint", "max_age", "access_type", "include_granted_scopes"];
        Assert.Empty(names.Intersect(forbidden, StringComparer.Ordinal));
        Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void MaxAge_IsCarriedOnReauthenticationOnly_AndOnlyWhenTheDecisionSaysDv26IsTrue()
    {
        Assert.Contains(Parameters(Build(reauthentication: true, sendMaxAge: true)), p => p.Key == "max_age" && p.Value == GoogleAuthorizationRequest.MaxAgeValue);
        Assert.DoesNotContain(Parameters(Build(reauthentication: true, sendMaxAge: false)), p => p.Key == "max_age");
        Assert.DoesNotContain(Parameters(Build(reauthentication: false, sendMaxAge: true)), p => p.Key == "max_age");
    }

    [Fact]
    public void MaxAge_IsNotCarriedByTheDefaultRequest_BecauseDv26IsNotObserved()
    {
        Assert.False(GoogleEndpoints.SendsMaxAge);
        Assert.DoesNotContain(Parameters(Build(reauthentication: true)), p => p.Key == "max_age");
    }

    [Fact]
    public void ValuesWithReservedCharacters_AreEscapedAndComeBackEqual()
    {
        Uri uri = GoogleAuthorizationRequest.Build(GoogleTestSetup.ClientId, GoogleTestSetup.Domain, "a b&c=d+e/f?g#h", "n&o=p", Verifier, reauthentication: true);

        Dictionary<string, string> parameters = Parameters(uri).ToDictionary(p => p.Key, p => p.Value);
        Assert.Equal("a b&c=d+e/f?g#h", parameters["state"]);
        Assert.Equal("n&o=p", parameters["nonce"]);
        Assert.Equal(string.Empty, uri.Fragment);
        Assert.Equal(9, parameters.Count);
    }

    [Fact]
    public void CodeChallenge_IsTheS256OfTheVerifier_RfcVector()
    {
        // RFC 7636 appendix B.
        Assert.Equal("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM", GoogleAuthorizationRequest.CodeChallenge(Verifier));
    }

    [Theory]
    [InlineData("", "state", "nonce", "verifier")]
    [InlineData("client", " ", "nonce", "verifier")]
    [InlineData("client", "state", "", "verifier")]
    [InlineData("client", "state", "nonce", " ")]
    public void AMissingValue_IsRefused(string clientId, string state, string nonce, string verifier)
    {
        Assert.ThrowsAny<ArgumentException>(() => GoogleAuthorizationRequest.Build(clientId, GoogleTestSetup.Domain, state, nonce, verifier, reauthentication: false));
    }
}
