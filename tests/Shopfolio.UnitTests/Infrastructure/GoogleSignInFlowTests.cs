using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The Google flow's start and end without a server (T515 criterion 6 and 9; KK6, KK9; 05 6.7.12; 09 15.1 item 7): the cookie holds the flow's values encrypted, the state carries the kind, the start moment
/// and the return path, the values are used once, and a foreign, damaged, renewed or too old return has no flow.
/// </summary>
public sealed class GoogleSignInFlowTests
{
    private static readonly DateTimeOffset _start = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private readonly ManualTimeProvider _time = new(_start);
    private readonly FakeGoogleIdentityGateway _gateway = new();
    private readonly GoogleSignInFlow _flow;

    public GoogleSignInFlowTests()
    {
        var protectedValues = new ProtectedValues(new EphemeralDataProtectionProvider());
        _flow = new GoogleSignInFlow(
            new BrowserCookies(protectedValues),
            protectedValues,
            Options.Create(new GoogleOptions { ClientId = "client-x" }),
            Options.Create(new SiteOptions { Domain = "shop.example.com" }),
            _time,
            _gateway);
    }

    [Fact]
    public void Start_WritesTheFlowCookieAndAnAddressWithStateNonceAndTheProofKey()
    {
        var context = new DefaultHttpContext();

        Uri address = _flow.Start(context, GoogleFlowKind.SignInOrRegistration, null, "/urun/masa");

        Assert.StartsWith("https://accounts.google.com/o/oauth2/v2/auth?", address.AbsoluteUri, StringComparison.Ordinal);
        Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(address.Query);
        Assert.Equal("client-x", query["client_id"].ToString());
        Assert.Equal("S256", query["code_challenge_method"].ToString());
        Assert.False(string.IsNullOrEmpty(query["code_challenge"]));
        Assert.False(string.IsNullOrEmpty(query["nonce"]));
        Assert.False(string.IsNullOrEmpty(query["state"]));
        Assert.DoesNotContain("/urun/masa", address.Query, StringComparison.Ordinal);
        string header = context.Response.Headers.SetCookie.ToString();
        Assert.StartsWith(CookieNames.ExternalLogin + "=", header, StringComparison.Ordinal);
        Assert.DoesNotContain(query["nonce"].ToString(), header, StringComparison.Ordinal);
    }

    [Fact]
    public void Consume_OnTheReturnOfTheSameFlow_GivesTheValuesTheKindAndTheReturnPath_AndDeletesTheCookie()
    {
        (HttpContext back, string state, Dictionary<string, StringValues> query) = StartAndReturn(GoogleFlowKind.Reauthentication, "profile-email", "/hesap?a=b|c");

        GoogleFlow? flow = _flow.Consume(back, state);

        Assert.NotNull(flow);
        Assert.Equal(GoogleFlowKind.Reauthentication, flow.State.Kind);
        Assert.Equal("profile-email", flow.State.Operation);
        Assert.Equal("/hesap?a=b|c", flow.State.ReturnUrl);
        Assert.Equal(_start, flow.State.StartedAt);
        Assert.Equal(query["nonce"].ToString(), flow.Values.Nonce);
        Assert.True(GoogleSignInFlow.NonceMatches(flow.Values, query["nonce"].ToString()));
        Assert.Equal(AuthorizationChallenge(flow.Values.CodeVerifier), query["code_challenge"].ToString());
        Assert.Contains(CookieNames.ExternalLogin + "=;", back.Response.Headers.SetCookie.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Consume_WithoutAnOperationOrAReturnPath_GivesNullForBoth()
    {
        (HttpContext back, string state, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, null);

        GoogleFlow? flow = _flow.Consume(back, state);

        Assert.NotNull(flow);
        Assert.Null(flow.State.Operation);
        Assert.Null(flow.State.ReturnUrl);
    }

    [Fact]
    public void Consume_WithoutTheCookie_ThatIsARenewedReturn_GivesNothing()
    {
        (_, string state, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, "/");
        var bare = new DefaultHttpContext();

        Assert.Null(_flow.Consume(bare, state));
    }

    [Fact]
    public void Consume_AFlowThatWasConsumed_CannotBeConsumedAgainFromTheSameCookieHeaderOfTheNextResponse()
    {
        (HttpContext back, string state, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, null);
        Assert.NotNull(_flow.Consume(back, state));

        // The browser took the deletion: the next return carries no cookie.
        var next = new DefaultHttpContext();

        Assert.Null(_flow.Consume(next, state));
    }

    [Fact]
    public void Consume_WithTheStateOfAnotherFlow_GivesNothing()
    {
        (HttpContext firstBack, _, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, null);
        (_, string otherState, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, null);

        Assert.Null(_flow.Consume(firstBack, otherState));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-protected-value")]
    public void Consume_WithAMissingOrDamagedState_GivesNothing(string? state)
    {
        (HttpContext back, _, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, null);

        Assert.Null(_flow.Consume(back, state));
    }

    [Fact]
    public void Consume_WithADamagedCookie_GivesNothing()
    {
        (_, string state, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, null);
        var back = new DefaultHttpContext();
        back.Request.Headers.Cookie = CookieNames.ExternalLogin + "=garbage";

        Assert.Null(_flow.Consume(back, state));
    }

    [Fact]
    public void Consume_AfterTenMinutes_GivesNothing_AndWithinThemItDoes()
    {
        (HttpContext early, string state, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, null);
        _time.Advance(TimeSpan.FromMinutes(9));
        Assert.NotNull(_flow.Consume(early, state));

        (HttpContext late, string lateState, _) = StartAndReturn(GoogleFlowKind.SignInOrRegistration, null, null);
        _time.Advance(TimeSpan.FromMinutes(11));
        Assert.Null(_flow.Consume(late, lateState));
    }

    [Theory]
    [InlineData("other")]
    [InlineData("")]
    [InlineData(null)]
    public void NonceMatches_OnADifferentOrMissingNonce_IsFalse(string? returned)
    {
        var values = new GoogleFlowValues("id", "the-nonce", "verifier");

        Assert.False(GoogleSignInFlow.NonceMatches(values, returned));
        Assert.True(GoogleSignInFlow.NonceMatches(values, "the-nonce"));
    }

    [Fact]
    public async Task VerifyAsync_OnAVerifiedIdentityWithTheFlowsNonce_GivesTheIdentity_AndSendsTheCodeWithTheFlowsVerifier()
    {
        var values = new GoogleFlowValues("id", "the-nonce", "the-verifier");
        _gateway.Enqueue(new GoogleIdentityResult.Succeeded("a@example.com", true, "Ada", null, null, "the-nonce"));

        GoogleIdentityResult.Succeeded? identity = await _flow.VerifyAsync(values, "code-1", TestContext.Current.CancellationToken);

        Assert.NotNull(identity);
        Assert.Equal("a@example.com", identity.Email);
        FakeCall call = Assert.Single(_gateway.Calls);
        Assert.Equal(new IdentityCodeExchange("code-1", "the-verifier"), call.Argument);
    }

    [Fact]
    public async Task VerifyAsync_OnAnotherNonceOnAFailureOrWhenGoogleIsUnavailable_GivesNull()
    {
        var values = new GoogleFlowValues("id", "the-nonce", "the-verifier");
        _gateway.Enqueue(
            new GoogleIdentityResult.Succeeded("a@example.com", true, null, null, null, "other-nonce"),
            new GoogleIdentityResult.Failed("invalid"),
            new GoogleIdentityResult.Unavailable("timeout"));

        Assert.Null(await _flow.VerifyAsync(values, "c1", TestContext.Current.CancellationToken));
        Assert.Null(await _flow.VerifyAsync(values, "c2", TestContext.Current.CancellationToken));
        Assert.Null(await _flow.VerifyAsync(values, "c3", TestContext.Current.CancellationToken));
    }

    private (HttpContext Back, string State, Dictionary<string, StringValues> Query) StartAndReturn(GoogleFlowKind kind, string? operation, string? returnUrl)
    {
        var start = new DefaultHttpContext();
        Uri address = _flow.Start(start, kind, operation, returnUrl);
        Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(address.Query);
        var back = new DefaultHttpContext();
        back.Request.Headers.Cookie = start.Response.Headers.SetCookie.ToString().Split(';')[0];
        return (back, query["state"].ToString(), query);
    }

    private static string AuthorizationChallenge(string verifier) => GoogleAuthorizationRequest.CodeChallenge(verifier);
}
