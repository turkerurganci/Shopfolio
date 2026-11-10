using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// Google's gateway (T409 criteria 1, 2, 4-7; KK1, KK2, KK4-KK7; 08 2.3; 09 11.3 item 5): the code exchange goes as a POST with the form's six fields and nothing is asked of
/// <c>userinfo</c>; with a setting missing no call is made; the identity token is verified for the seven states of 09 11.3 item 5; the key set is cached and an unknown key id is the
/// manager's refresh, once, with no hand-written retry; the result carries the mapped fields and the nonce and never the subject; a refused, timed-out or errored call is "unavailable"
/// after one call, an answer outside the table is a failure. The wire is a hand-written handler; no socket is opened and no test reaches Google.
/// </summary>
public sealed class GoogleIdentityGatewayTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Exchange_IsAPost_ToTheFixedTokenAddress_WithTheSixFormFields_AndNoUserinfoCall()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(key), key));

        GoogleIdentityResult result = await rig.VerifyAsync(Ct);

        Assert.IsType<GoogleIdentityResult.Succeeded>(result);
        GoogleRequest exchange = Assert.Single(rig.Wire.Requests, r => r.IsExchange);
        Assert.Equal("application/x-www-form-urlencoded", exchange.ContentType);
        Dictionary<string, string> form = exchange.Form();
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = GoogleTestSetup.Exchange.AuthorizationCode,
                ["client_id"] = GoogleTestSetup.ClientId,
                ["client_secret"] = GoogleTestSetup.ClientSecret,
                ["redirect_uri"] = "https://shop.example.com/giris/google",
                ["code_verifier"] = GoogleTestSetup.Exchange.CodeVerifier,
            },
            form);
        Assert.DoesNotContain(rig.Wire.Requests, r => r.Uri.Host.Contains("openidconnect", StringComparison.Ordinal) || r.Uri.AbsolutePath.Contains("userinfo", StringComparison.Ordinal));
        Assert.All(rig.Wire.Requests, r => Assert.Equal(Uri.UriSchemeHttps, r.Uri.Scheme));
    }

    [Theory]
    [InlineData("", GoogleTestSetup.ClientSecret)]
    [InlineData(GoogleTestSetup.ClientId, "")]
    [InlineData("", "")]
    [InlineData("  ", GoogleTestSetup.ClientSecret)]
    public async Task WithAClientSettingMissing_NoCallIsMade_AndTheResultIsAFailure(string clientId, string clientSecret)
    {
        using GoogleRig rig = new(GoogleTestSetup.Script("unused"), new GoogleOptions { ClientId = clientId, ClientSecret = clientSecret });

        GoogleIdentityResult result = await rig.VerifyAsync(Ct);

        Assert.Equal(new GoogleIdentityResult.Failed(GoogleIdentityMapping.NotConfiguredReason), result);
        Assert.Empty(rig.Wire.Requests);
    }

    [Fact]
    public async Task ValidToken_GivesTheMappedFields_AndTheNonce_AndNoSubject()
    {
        using GoogleTestKey key = new("k1");
        long authTime = DateTimeOffset.UtcNow.AddSeconds(-20).ToUnixTimeSeconds();
        string jwtText = GoogleTestSetup.Token(key, GoogleTestSetup.Claims(c => c["auth_time"] = authTime));
        using GoogleRig rig = new(GoogleTestSetup.Script(jwtText, key));

        GoogleIdentityResult result = await rig.VerifyAsync(Ct);

        Assert.Equal(
            new GoogleIdentityResult.Succeeded("ayse@example.com", true, "Ayşe Yılmaz", "example.com", DateTimeOffset.FromUnixTimeSeconds(authTime), GoogleTestSetup.Nonce),
            result);
        Assert.DoesNotContain(GoogleTestSetup.SubjectValue, result.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(typeof(GoogleIdentityResult.Succeeded).GetProperties(), p => p.Name is "Subject" or "Sub" or "Picture" or "GivenName" or "FamilyName");
        Assert.DoesNotContain(rig.Harness.Lines, line => line.Contains(GoogleTestSetup.SubjectValue, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidToken_WithoutOptionalFields_GivesNullsAndNoAuthTime_AndEmailVerifiedAsGiven()
    {
        using GoogleTestKey key = new("k1");
        string jwtText = GoogleTestSetup.Token(
            key,
            GoogleTestSetup.Claims(c =>
            {
                c.Remove("name");
                c.Remove("hd");
                c.Remove("auth_time");
                c["email_verified"] = false;
            }));
        using GoogleRig rig = new(GoogleTestSetup.Script(jwtText, key));

        GoogleIdentityResult result = await rig.VerifyAsync(Ct);

        Assert.Equal(new GoogleIdentityResult.Succeeded("ayse@example.com", false, null, null, null, GoogleTestSetup.Nonce), result);
    }

    [Fact]
    public async Task ValidToken_WithoutANonce_GivesAnEmptyNonce_SoTheCoresComparisonFails()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(key, GoogleTestSetup.Claims(c => c.Remove("nonce"))), key));

        GoogleIdentityResult.Succeeded result = Assert.IsType<GoogleIdentityResult.Succeeded>(await rig.VerifyAsync(Ct));

        Assert.Equal(string.Empty, result.Nonce);
    }

    [Fact]
    public async Task TokenWithADifferentAlgorithm_Fails_AsASignatureThatDoesNotHold()
    {
        using GoogleTestKey key = new("k1");
        string jwtText = GoogleTestSetup.Token(key, algorithm: SecurityAlgorithms.RsaSha384);
        using GoogleRig rig = new(GoogleTestSetup.Script(jwtText, key));

        Assert.Equal(new GoogleIdentityResult.Failed("token:signature"), await rig.VerifyAsync(Ct));
    }

    [Fact]
    public async Task UnsignedToken_Fails()
    {
        using GoogleTestKey key = new("k1");
        string jwtText = GoogleTestSetup.Token(key, signed: false);
        using GoogleRig rig = new(GoogleTestSetup.Script(jwtText, key));

        GoogleIdentityResult result = await rig.VerifyAsync(Ct);

        Assert.IsType<GoogleIdentityResult.Failed>(result);
    }

    [Fact]
    public async Task TokenSignedByAnotherKeyWithTheSameKeyId_Fails()
    {
        using GoogleTestKey published = new("k1");
        using GoogleTestKey other = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(other), published));

        Assert.Equal(new GoogleIdentityResult.Failed("token:signature"), await rig.VerifyAsync(Ct));
    }

    [Fact]
    public async Task TokenWithAnUnknownKeyId_AsksTheManagersRefresh_OnceAndTheNextCallSeesTheNewSet()
    {
        using GoogleTestKey original = new("k1");
        using GoogleTestKey rotated = new("k2");
        int keyFetches = 0;
        string jwtText = GoogleTestSetup.Token(rotated);
        using GoogleRig rig = new(request => request switch
        {
            { IsExchange: true } => GoogleAnswer.Exchanged(jwtText),
            { IsDiscovery: true } => GoogleAnswer.Ok(GoogleTestSetup.DiscoveryDocument()),
            { IsKeys: true } => GoogleAnswer.Ok(Interlocked.Increment(ref keyFetches) == 1 ? GoogleTestSetup.KeySet(original) : GoogleTestSetup.KeySet(original, rotated)),
            _ => new GoogleAnswer.Reply(HttpStatusCode.NotFound, "{}"),
        });

        OpenIdConnectConfiguration warmed = await WarmedConfigurationAsync(rig);
        GoogleIdentityResult first = await rig.VerifyAsync(Ct);
        await NextConfigurationAfterAsync(rig, warmed);
        GoogleIdentityResult second = await rig.VerifyAsync(Ct);

        // Observed with the library of 09 3 (8.23.0; DV-31): a key id missing from the cached set is the manager's refresh request, and the refresh runs behind the call - the call
        // that found the key missing fails, the next one sees the new set. No retry is written here: the exchange (the spent code) goes once per call.
        Assert.StartsWith("token:", Assert.IsType<GoogleIdentityResult.Failed>(first).ReasonCode, StringComparison.Ordinal);
        Assert.IsType<GoogleIdentityResult.Succeeded>(second);
        Assert.Equal(2, rig.Wire.Requests.Count(r => r.IsKeys));
        Assert.Equal(2, rig.Wire.Requests.Count(r => r.IsExchange));
    }

    /// <summary>Loads the discovery document and the key set into the manager's cache before the call, so the configuration the call starts from is known before any refresh can be asked for.</summary>
    private static async Task<OpenIdConnectConfiguration> WarmedConfigurationAsync(GoogleRig rig)
    {
        ConfigurationManager<OpenIdConnectConfiguration> manager = rig.Services.GetRequiredService<ConfigurationManager<OpenIdConnectConfiguration>>();
        return await manager.GetConfigurationAsync(Ct);
    }

    /// <summary>Waits, with no clock, until the manager holds a configuration other than the one it held: the refresh that a missing key id asked for has been applied (a fuse ends the wait on a failure).</summary>
    private static async Task NextConfigurationAfterAsync(GoogleRig rig, OpenIdConnectConfiguration before)
    {
        ConfigurationManager<OpenIdConnectConfiguration> manager = rig.Services.GetRequiredService<ConfigurationManager<OpenIdConnectConfiguration>>();
        using var fuse = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        fuse.CancelAfter(TimeSpan.FromSeconds(10));
        while (ReferenceEquals(await manager.GetConfigurationAsync(fuse.Token), before))
        {
            fuse.Token.ThrowIfCancellationRequested();
            await Task.Yield();
        }
    }

    [Fact]
    public async Task TokenWithAKeyIdThatIsStillUnknownAfterTheRefresh_Fails_AndFurtherCallsDoNotFetchAgain()
    {
        using GoogleTestKey published = new("k1");
        using GoogleTestKey stranger = new("k9");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(stranger), published));

        OpenIdConnectConfiguration warmed = await WarmedConfigurationAsync(rig);
        GoogleIdentityResult first = await rig.VerifyAsync(Ct);
        await NextConfigurationAfterAsync(rig, warmed);
        GoogleIdentityResult second = await rig.VerifyAsync(Ct);
        GoogleIdentityResult third = await rig.VerifyAsync(Ct);

        Assert.All([first, second, third], r => Assert.StartsWith("token:", Assert.IsType<GoogleIdentityResult.Failed>(r).ReasonCode, StringComparison.Ordinal));
        Assert.Equal(2, rig.Wire.Requests.Count(r => r.IsKeys));
        Assert.Equal(3, rig.Wire.Requests.Count(r => r.IsExchange));
    }

    [Theory]
    [InlineData("https://accounts.evil.example", "token:issuer")]
    [InlineData("accounts.google.com", "token:issuer")]
    public async Task TokenWithAnotherIssuer_Fails_AndOnlyTheDiscoveryDocumentsFormIsAccepted(string issuer, string reason)
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(key, issuer: issuer), key));

        Assert.Equal(new GoogleIdentityResult.Failed(reason), await rig.VerifyAsync(Ct));
    }

    [Fact]
    public async Task TokenForAnotherAudience_Fails()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(key, audience: "someone-elses-client-id"), key));

        Assert.Equal(new GoogleIdentityResult.Failed("token:audience"), await rig.VerifyAsync(Ct));
    }

    [Fact]
    public async Task ExpiredToken_Fails()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(key, expiresIn: -GoogleEndpoints.IdentityTokenClockSkew - TimeSpan.FromSeconds(30)), key));

        Assert.Equal(new GoogleIdentityResult.Failed("token:expired"), await rig.VerifyAsync(Ct));
    }

    [Fact]
    public async Task TokenThatExpiredWithinTheClockSkew_IsAccepted()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(key, expiresIn: -GoogleEndpoints.IdentityTokenClockSkew + TimeSpan.FromSeconds(15)), key));

        Assert.IsType<GoogleIdentityResult.Succeeded>(await rig.VerifyAsync(Ct));
    }

    [Fact]
    public async Task TokenWithoutAnExpiry_Fails()
    {
        using GoogleTestKey key = new("k1");
        string jwtText = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler().CreateToken(
            "{\"iss\":\"" + GoogleTestSetup.Issuer + "\",\"aud\":\"" + GoogleTestSetup.ClientId + "\",\"email\":\"ayse@example.com\"}", key.Credentials());
        using GoogleRig rig = new(GoogleTestSetup.Script(jwtText, key));

        Assert.Equal(new GoogleIdentityResult.Failed("token:no-expiration"), await rig.VerifyAsync(Ct));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TokenWithoutAnEmail_OrAnEmptyOne_Fails(bool empty)
    {
        using GoogleTestKey key = new("k1");
        string jwtText = GoogleTestSetup.Token(key, GoogleTestSetup.Claims(c =>
        {
            c.Remove("email");
            if (empty)
            {
                c["email"] = " ";
            }
        }));
        using GoogleRig rig = new(GoogleTestSetup.Script(jwtText, key));

        Assert.Equal(new GoogleIdentityResult.Failed(GoogleIdentityMapping.EmailMissingReason), await rig.VerifyAsync(Ct));
    }

    [Fact]
    public async Task TheKeySet_IsCached_TwoVerificationsFetchTheDiscoveryDocumentAndTheKeysOnce()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(key), key));

        Assert.IsType<GoogleIdentityResult.Succeeded>(await rig.VerifyAsync(Ct));
        Assert.IsType<GoogleIdentityResult.Succeeded>(await rig.VerifyAsync(Ct));

        Assert.Equal(1, rig.Wire.Requests.Count(r => r.IsDiscovery));
        Assert.Equal(1, rig.Wire.Requests.Count(r => r.IsKeys));
        Assert.Equal(2, rig.Wire.Requests.Count(r => r.IsExchange));
    }

    [Fact]
    public async Task WarmCache_MakesOneCall_TheExchange()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(GoogleTestSetup.Script(GoogleTestSetup.Token(key), key));
        _ = await rig.VerifyAsync(Ct);
        int before = rig.Wire.Requests.Count;

        _ = await rig.VerifyAsync(Ct);

        Assert.Equal(before + 1, rig.Wire.Requests.Count);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "{\"error\":\"invalid_grant\",\"error_description\":\"Bad Request\"}", "google-error:invalid_grant")]
    [InlineData(HttpStatusCode.Unauthorized, "{\"error\":\"invalid_client\"}", "google-error:invalid_client")]
    [InlineData(HttpStatusCode.InternalServerError, "<html>boom</html>", "http:500")]
    [InlineData(HttpStatusCode.BadRequest, "{\"error\":\"Some Error With Spaces: client-secret\"}", "http:400")]
    public async Task ExchangeAnswerOfAnError_IsUnavailable_AfterOneCall_AndNoRetry(HttpStatusCode status, string body, string reason)
    {
        using GoogleRig rig = new(request => request.IsExchange ? new GoogleAnswer.Reply(status, body) : new GoogleAnswer.Reply(HttpStatusCode.NotFound, "{}"));

        GoogleIdentityResult result = await rig.VerifyAsync(Ct);

        Assert.Equal(new GoogleIdentityResult.Unavailable(reason), result);
        GoogleRequest only = Assert.Single(rig.Wire.Requests);
        Assert.True(only.IsExchange);
        string line = Assert.Single(rig.Harness.Lines);
        Assert.Contains(reason, line, StringComparison.Ordinal);
        Assert.DoesNotContain("client-secret", line.Replace(reason, string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExchangeThatCannotConnect_IsUnavailable_AfterOneCall()
    {
        using GoogleRig rig = new(_ => new GoogleAnswer.Refuse());

        GoogleIdentityResult result = await rig.VerifyAsync(Ct);

        Assert.Equal(new GoogleIdentityResult.Unavailable(ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError)), result);
        Assert.Single(rig.Wire.Requests);
    }

    [Fact]
    public async Task ExchangeThatHangs_EndsAtTheUserWaitingLimit_AsUnavailable_AndGoesOnce()
    {
        using GoogleRig rig = new(_ => new GoogleAnswer.Hang());
        Task<GoogleIdentityResult> pending = rig.VerifyAsync(Ct);
        await rig.Wire.NextRequestAsync(Ct);

        rig.Harness.Time.Advance(ExternalCallTimeouts.UserWaiting - TimeSpan.FromTicks(1));
        Assert.False(pending.IsCompleted);
        rig.Harness.Time.Advance(TimeSpan.FromTicks(1));
        GoogleIdentityResult result = await pending;

        Assert.Equal(new GoogleIdentityResult.Unavailable(ExternalCallFailure.TimeoutReason), result);
        Assert.Single(rig.Wire.Requests);
    }

    [Theory]
    [InlineData("{}", GoogleIdentityMapping.NoIdTokenReason)]
    [InlineData("{\"id_token\":\"\"}", GoogleIdentityMapping.NoIdTokenReason)]
    [InlineData("{\"id_token\":7}", GoogleIdentityMapping.NoIdTokenReason)]
    [InlineData("[]", GoogleIdentityMapping.MalformedAnswerReason)]
    [InlineData("not json", GoogleIdentityMapping.MalformedAnswerReason)]
    [InlineData("", GoogleIdentityMapping.MalformedAnswerReason)]
    public async Task ExchangeAnswerOutsideTheTable_IsAFailure_NotAnOutage(string body, string reason)
    {
        using GoogleRig rig = new(request => request.IsExchange ? GoogleAnswer.Ok(body) : new GoogleAnswer.Reply(HttpStatusCode.NotFound, "{}"));

        Assert.Equal(new GoogleIdentityResult.Failed(reason), await rig.VerifyAsync(Ct));
        Assert.Single(rig.Wire.Requests);
    }

    [Fact]
    public async Task DiscoveryDocumentThatCannotBeGot_IsUnavailable_AndTheSpentCodeIsNotSentAgain()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(request => request switch
        {
            { IsExchange: true } => GoogleAnswer.Exchanged(GoogleTestSetup.Token(key)),
            _ => new GoogleAnswer.Reply(HttpStatusCode.ServiceUnavailable, "{}"),
        });

        GoogleIdentityResult result = await rig.VerifyAsync(Ct);

        Assert.Equal(new GoogleIdentityResult.Unavailable(GoogleIdentityMapping.ConfigurationReason), result);
        Assert.Equal(1, rig.Wire.Requests.Count(r => r.IsExchange));
    }

    [Fact]
    public async Task KeySetThatHangs_EndsAtItsOwnLimit_AsUnavailable()
    {
        using GoogleTestKey key = new("k1");
        using GoogleRig rig = new(request => request switch
        {
            { IsExchange: true } => GoogleAnswer.Exchanged(GoogleTestSetup.Token(key)),
            { IsDiscovery: true } => GoogleAnswer.Ok(GoogleTestSetup.DiscoveryDocument()),
            _ => new GoogleAnswer.Hang(),
        });
        Task<GoogleIdentityResult> pending = rig.VerifyAsync(Ct);
        await rig.Wire.NextRequestAsync(Ct);
        await rig.Wire.NextRequestAsync(Ct);
        await rig.Wire.NextRequestAsync(Ct);

        rig.Harness.Time.Advance(ExternalCallTimeouts.UserWaiting);
        GoogleIdentityResult result = await pending;

        // The read's own limit and the step's limit pass at the same instant on the fake clock: whichever the call sees first names the reason.
        GoogleIdentityResult.Unavailable unavailable = Assert.IsType<GoogleIdentityResult.Unavailable>(result);
        Assert.Contains(unavailable.ReasonCode, new[] { ExternalCallFailure.TimeoutReason, GoogleIdentityMapping.ConfigurationReason });
        Assert.Equal(1, rig.Wire.Requests.Count(r => r.IsExchange));
        Assert.Equal(1, rig.Wire.Requests.Count(r => r.IsKeys));
    }

    [Fact]
    public async Task AFailureLine_CarriesTheJobTheKindAndTheReason_AndNoSecretCodeVerifierOrToken()
    {
        using GoogleTestKey key = new("k1");
        string jwtText = GoogleTestSetup.Token(key, audience: "someone-elses-client-id");
        using GoogleRig rig = new(GoogleTestSetup.Script(jwtText, key));

        _ = await rig.VerifyAsync(Ct);

        string line = Assert.Single(rig.Harness.Lines);
        Assert.Contains("IdentityVerify", line, StringComparison.Ordinal);
        Assert.Contains("token:audience", line, StringComparison.Ordinal);
        foreach (string secret in new[] { GoogleTestSetup.ClientSecret, GoogleTestSetup.Exchange.AuthorizationCode, GoogleTestSetup.Exchange.CodeVerifier, token, GoogleTestSetup.SubjectValue, "ayse@example.com" })
        {
            Assert.DoesNotContain(secret, line, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Registration_MakesOneManagerOverTheFixedDiscoveryAddress_AndReadsNothingWhenResolved()
    {
        using GoogleRig rig = new(GoogleTestSetup.Script("unused"));

        ConfigurationManager<OpenIdConnectConfiguration> manager = rig.Services.GetRequiredService<ConfigurationManager<OpenIdConnectConfiguration>>();

        Assert.Same(manager, rig.Services.GetRequiredService<ConfigurationManager<OpenIdConnectConfiguration>>());
        Assert.Equal(GoogleEndpoints.Discovery, manager.MetadataAddress);
        Assert.IsType<GoogleIdentityGateway>(rig.Gateway);
        Assert.Empty(rig.Wire.Requests);
    }

    [Fact]
    public void ValidationParameters_AcceptOnlyRs256_RequireASignature_AndTheClientIdAsAudience()
    {
        using GoogleRig rig = new(GoogleTestSetup.Script("unused"));
        GoogleIdentityGateway gateway = Assert.IsType<GoogleIdentityGateway>(rig.Gateway);

        var parameters = gateway.ValidationParameters(GoogleTestSetup.ClientId);

        Assert.Equal(["RS256"], parameters.ValidAlgorithms);
        Assert.True(parameters.RequireSignedTokens);
        Assert.True(parameters.ValidateIssuerSigningKey);
        Assert.True(parameters.ValidateIssuer);
        Assert.True(parameters.ValidateAudience);
        Assert.Equal(GoogleTestSetup.ClientId, parameters.ValidAudience);
        Assert.True(parameters.ValidateLifetime);
        Assert.True(parameters.RequireExpirationTime);
        Assert.Equal(TimeSpan.FromSeconds(60), parameters.ClockSkew);
        Assert.Null(parameters.IssuerSigningKeys);
        Assert.Null(parameters.IssuerSigningKeyResolver);
        Assert.Null(parameters.ValidIssuer);
        Assert.Null(parameters.ValidIssuers);
        Assert.NotNull(parameters.ConfigurationManager);
    }

    [Fact]
    public void TheAddressesAreFixed_AndTheDocumentReaderRequiresHttps()
    {
        Assert.Equal("https://accounts.google.com/o/oauth2/v2/auth", GoogleEndpoints.Authorization);
        Assert.Equal("https://oauth2.googleapis.com/token", GoogleEndpoints.Token);
        Assert.Equal("https://accounts.google.com/.well-known/openid-configuration", GoogleEndpoints.Discovery);
        Assert.Equal("https://shop.example.com/giris/google", GoogleEndpoints.ReturnAddress("shop.example.com"));
        Assert.All([GoogleEndpoints.Authorization, GoogleEndpoints.Token, GoogleEndpoints.Discovery], a => Assert.StartsWith("https://", a, StringComparison.Ordinal));
        Assert.False(GoogleEndpoints.SendsMaxAge);
        Assert.Equal(TimeSpan.FromSeconds(60), GoogleEndpoints.IdentityTokenClockSkew);
    }

    [Fact]
    public void ComposeExchange_PutsTheSecretInTheBodyOnly()
    {
        using HttpRequestMessage request = GoogleIdentityGateway.ComposeExchange(GoogleTestSetup.Exchange, GoogleTestSetup.Options(), GoogleTestSetup.Domain);

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(GoogleEndpoints.Token, request.RequestUri!.AbsoluteUri);
        Assert.Null(request.Headers.Authorization);
        Assert.DoesNotContain(GoogleTestSetup.ClientSecret, request.RequestUri.AbsoluteUri, StringComparison.Ordinal);
        Assert.Empty(request.Headers);
    }
}
