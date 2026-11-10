using System.Net;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The pure mappings of the Google call (T409 criteria 4, 6, 7; 08 2.3 field mapping and error table): the payload of a verified identity token to the result, the answer of the
/// code exchange to the identity token or the failure, and the failure of a validation to a short reason code. None of them calls anything.
/// </summary>
public sealed class GoogleIdentityMappingTests
{
    private static GoogleIdentityResult Read(string json) => GoogleIdentityMapping.ReadIdentity(Encoding.UTF8.GetBytes(json));

    [Fact]
    public void Identity_CarriesOnlyTheSixFields_AndNoSubject()
    {
        GoogleIdentityResult result = Read(
            "{\"sub\":\"1234567890\",\"email\":\"ayse@example.com\",\"email_verified\":true,\"name\":\"Ayse\",\"hd\":\"example.com\",\"auth_time\":1700000000,\"nonce\":\"n-1\",\"picture\":\"p\"}");

        GoogleIdentityResult.Succeeded identity = Assert.IsType<GoogleIdentityResult.Succeeded>(result);
        Assert.Equal("ayse@example.com", identity.Email);
        Assert.True(identity.EmailVerified);
        Assert.Equal("Ayse", identity.Name);
        Assert.Equal("example.com", identity.HostedDomain);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), identity.AuthTime);
        Assert.Equal("n-1", identity.Nonce);
        Assert.DoesNotContain(typeof(GoogleIdentityResult.Succeeded).GetProperties(), p => p.Name.Contains("Sub", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("\"email_verified\":true", true)]
    [InlineData("\"email_verified\":\"true\"", true)]
    [InlineData("\"email_verified\":\"True\"", false)]
    [InlineData("\"email_verified\":false", false)]
    [InlineData("\"email_verified\":\"false\"", false)]
    [InlineData("\"email_verified\":1", false)]
    [InlineData("\"other\":1", false)]
    public void EmailVerified_IsTrueOnlyForABooleanTrueOrTheTextTrue(string field, bool expected)
    {
        GoogleIdentityResult.Succeeded identity = Assert.IsType<GoogleIdentityResult.Succeeded>(Read("{\"email\":\"a@b.example\"," + field + "}"));

        Assert.Equal(expected, identity.EmailVerified);
    }

    [Theory]
    [InlineData("\"auth_time\":1700000000", true)]
    [InlineData("\"auth_time\":\"1700000000\"", true)]
    [InlineData("\"auth_time\":\"-5\"", false)]
    [InlineData("\"auth_time\":-5", false)]
    [InlineData("\"auth_time\":99999999999999", false)]
    [InlineData("\"auth_time\":\"soon\"", false)]
    [InlineData("\"auth_time\":1.5", false)]
    [InlineData("\"auth_time\":null", false)]
    [InlineData("\"other\":1", false)]
    public void AuthTime_IsAReadableSecondCountOrAbsent(string field, bool present)
    {
        GoogleIdentityResult.Succeeded identity = Assert.IsType<GoogleIdentityResult.Succeeded>(Read("{\"email\":\"a@b.example\"," + field + "}"));

        Assert.Equal(present, identity.AuthTime is not null);
    }

    [Fact]
    public void Identity_WithoutANonce_HasAnEmptyOne()
    {
        GoogleIdentityResult.Succeeded identity = Assert.IsType<GoogleIdentityResult.Succeeded>(Read("{\"email\":\"a@b.example\"}"));

        Assert.Equal(string.Empty, identity.Nonce);
        Assert.Null(identity.Name);
        Assert.Null(identity.HostedDomain);
    }

    [Theory]
    [InlineData("{\"email\":\"\"}")]
    [InlineData("{\"email\":\"   \"}")]
    [InlineData("{\"name\":\"Ayse\"}")]
    [InlineData("{\"email\":5}")]
    public void Identity_WithoutAUsableEmail_Fails(string json)
    {
        Assert.Equal(new GoogleIdentityResult.Failed(GoogleIdentityMapping.EmailMissingReason), Read(json));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("")]
    public void Payload_ThatIsNotAnObject_Fails(string json)
    {
        Assert.Equal(new GoogleIdentityResult.Failed(GoogleIdentityMapping.InvalidTokenReason), Read(json));
    }

    [Fact]
    public void ExchangeAnswer_WithAnIdentityToken_GivesTheToken()
    {
        GoogleExchangeOutcome outcome = GoogleIdentityMapping.ReadExchangeAnswer(HttpStatusCode.OK, "{\"id_token\":\"abc.def.ghi\",\"access_token\":\"x\"}");

        Assert.Equal("abc.def.ghi", outcome.IdToken);
        Assert.Null(outcome.Failure);
    }

    [Theory]
    [InlineData("{\"access_token\":\"x\"}", GoogleIdentityMapping.NoIdTokenReason)]
    [InlineData("{\"id_token\":\"\"}", GoogleIdentityMapping.NoIdTokenReason)]
    [InlineData("[1]", GoogleIdentityMapping.MalformedAnswerReason)]
    [InlineData("not json", GoogleIdentityMapping.MalformedAnswerReason)]
    public void ExchangeAnswer_WithoutAnIdentityToken_IsAFailureNotAnOutage(string body, string reason)
    {
        GoogleExchangeOutcome outcome = GoogleIdentityMapping.ReadExchangeAnswer(HttpStatusCode.OK, body);

        Assert.Null(outcome.IdToken);
        Assert.Equal(ExternalCallFailure.Unrecognized(reason), outcome.Failure);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "{\"error\":\"invalid_grant\"}", "google-error:invalid_grant")]
    [InlineData(HttpStatusCode.Unauthorized, "{\"error\":\"invalid_client\"}", "google-error:invalid_client")]
    [InlineData(HttpStatusCode.BadRequest, "{\"error\":\"Not A Code!\"}", "http:400")]
    [InlineData(HttpStatusCode.InternalServerError, "oops", "http:500")]
    public void ExchangeAnswer_OfAnError_IsAnOutageWithAShortReason(HttpStatusCode status, string body, string reason)
    {
        GoogleExchangeOutcome outcome = GoogleIdentityMapping.ReadExchangeAnswer(status, body);

        Assert.Null(outcome.IdToken);
        Assert.Equal(ExternalCallFailure.Unreachable(reason), outcome.Failure);
    }

    [Theory]
    [InlineData(typeof(SecurityTokenInvalidAlgorithmException), "token:alg")]
    [InlineData(typeof(SecurityTokenSignatureKeyNotFoundException), "token:key-not-found")]
    [InlineData(typeof(SecurityTokenInvalidSignatureException), "token:signature")]
    [InlineData(typeof(SecurityTokenInvalidIssuerException), "token:issuer")]
    [InlineData(typeof(SecurityTokenInvalidAudienceException), "token:audience")]
    [InlineData(typeof(SecurityTokenExpiredException), "token:expired")]
    [InlineData(typeof(SecurityTokenNoExpirationException), "token:no-expiration")]
    [InlineData(typeof(SecurityTokenNotYetValidException), "token:not-yet-valid")]
    [InlineData(typeof(SecurityTokenInvalidLifetimeException), "token:lifetime")]
    [InlineData(typeof(SecurityTokenMalformedException), "token:malformed")]
    [InlineData(typeof(SecurityTokenException), "token:invalid")]
    public void ValidationFailure_MapsToTheShortReasonOfItsKind(Type kind, string reason)
    {
        Exception exception = (Exception)Activator.CreateInstance(kind, "message that quotes the secret-looking text")!;

        string mapped = GoogleIdentityMapping.ValidationReason(exception);

        Assert.Equal(reason, mapped);
        Assert.DoesNotContain("secret-looking", mapped, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidationFailure_OfAnUnknownKindOrNone_IsTheGeneralReason()
    {
        Assert.Equal(GoogleIdentityMapping.InvalidTokenReason, GoogleIdentityMapping.ValidationReason(new FormatException()));
        Assert.Equal(GoogleIdentityMapping.InvalidTokenReason, GoogleIdentityMapping.ValidationReason(null));
    }

    [Fact]
    public void ConfigurationOutage_IsAFailureThatIsNotATokenFailure()
    {
        Assert.True(GoogleIdentityMapping.IsConfigurationOutage(new InvalidOperationException()));
        Assert.False(GoogleIdentityMapping.IsConfigurationOutage(new SecurityTokenExpiredException()));
        Assert.False(GoogleIdentityMapping.IsConfigurationOutage(null));
    }
}
