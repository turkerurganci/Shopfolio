using System.Net;

using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The client's make and the pure error mapping (T407 criteria 2 and 6, KK2, KK6; 09 11.1 item 3, 11.2 items 1, 2, 4 and 6): the library's retry is off, the limit is the job limit,
/// the region and the credential are the settings', the environment's credential and endpoint are not used; and the mapping of the send's answer is a pure
/// function whose rows are the table's.
/// </summary>
public sealed class SesClientAndMappingTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static EmailMessage Message() => new("alici@example.com", "Mağaza", null, "Konu", "<p>x</p>", "x");

    [Fact]
    public void CreateConfig_HasNoRetry_TheJobLimit_TheSettingsRegion_AndIgnoresTheEnvironmentsEndpoint()
    {
        AmazonSimpleEmailServiceV2Config config = SesClientFactory.CreateConfig(SesTestSetup.Options());

        Assert.Equal(0, config.MaxErrorRetry);
        Assert.Equal(TimeSpan.FromSeconds(30), config.Timeout);
        Assert.Equal(ExternalCallTimeouts.Job, config.Timeout);
        Assert.Equal(RegionEndpoint.EUCentral1, config.RegionEndpoint);
        Assert.True(config.IgnoreConfiguredEndpointUrls);
    }

    [Theory]
    [InlineData("eu-central-1")]
    [InlineData("us-east-1")]
    [InlineData("eu-west-3")]
    public void CreateConfig_TakesTheRegionOfTheSetting(string region)
    {
        EmailOptions options = SesTestSetup.Options();
        options.Region = region;

        Assert.Equal(RegionEndpoint.GetBySystemName(region), SesClientFactory.CreateConfig(options).RegionEndpoint);
    }

    [Fact]
    public void CreateCredentials_AreTheTwoLinesOfTheCredentialFile()
    {
        ImmutableCredentials credentials = SesClientFactory.CreateCredentials(SesTestSetup.Options()).GetCredentials();

        Assert.Equal(SesTestSetup.AccessKeyId, credentials.AccessKey);
        Assert.Equal(SesTestSetup.SecretKey, credentials.SecretKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("one-line-only")]
    [InlineData("a\nb\nc")]
    public void CreateCredentials_OfAContentThatIsNotTheTwoLines_Throws_WithoutTheValueInTheMessage(string content)
    {
        EmailOptions options = SesTestSetup.Options();
        options.Credential = content;

        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(() => SesClientFactory.CreateCredentials(options));

        Assert.Contains(EmailOptions.CredentialKey, failure.Message, StringComparison.Ordinal);
        if (content.Length > 0)
        {
            Assert.DoesNotContain(content, failure.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Client_SignsWithTheSettingsKey_AndGoesToTheSettingsRegionsEndpoint_EvenWhenTheEnvironmentHoldsAnotherIdentity()
    {
        string?[] names = ["AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY", "AWS_SESSION_TOKEN", "AWS_REGION", "AWS_DEFAULT_REGION", "AWS_ENDPOINT_URL", "AWS_PROFILE"];
        string?[] before = [.. names.Select(n => Environment.GetEnvironmentVariable(n!))];
        try
        {
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "AKIAENVIRONMENTKEY99");
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "environment-secret-key-ENVSECRET");
            Environment.SetEnvironmentVariable("AWS_REGION", "us-west-2");
            Environment.SetEnvironmentVariable("AWS_DEFAULT_REGION", "us-west-2");
            Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", "https://environment.example.invalid");

            using SesWire wire = new(_ => WireAnswer.Sent());
            using AmazonSimpleEmailServiceV2Client client = SesTestSetup.Client(wire);
            await client.SendEmailAsync(SesEmailGateway.Compose(Message(), SesTestSetup.SenderAddress, "shop.example.com"), Ct);

            WireRequest request = Assert.Single(wire.Requests);
            Assert.Equal("email.eu-central-1.amazonaws.com", request.Uri.Host);
            Assert.Equal("/v2/email/outbound-emails", request.Uri.AbsolutePath);
            string authorization = request.Headers["Authorization"];
            Assert.Contains("Credential=" + SesTestSetup.AccessKeyId + "/", authorization, StringComparison.Ordinal);
            Assert.Contains("/eu-central-1/ses/aws4_request", authorization, StringComparison.Ordinal);
            Assert.DoesNotContain("ENVIRONMENTKEY", authorization, StringComparison.Ordinal);
        }
        finally
        {
            for (int i = 0; i < names.Length; i++)
            {
                Environment.SetEnvironmentVariable(names[i]!, before[i]);
            }
        }
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "InternalFailure")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "ServiceUnavailable")]
    [InlineData(HttpStatusCode.TooManyRequests, "TooManyRequestsException")]
    [InlineData(HttpStatusCode.BadRequest, "ThrottlingException")]
    public async Task Client_AnswersThatTheLibraryWouldRetryByDefault_GoOnce(HttpStatusCode status, string errorType)
    {
        using SesWire wire = new(_ => WireAnswer.Error(status, errorType, "transient"));
        using AmazonSimpleEmailServiceV2Client client = SesTestSetup.Client(wire);

        await Assert.ThrowsAnyAsync<AmazonServiceException>(() =>
            client.SendEmailAsync(SesEmailGateway.Compose(Message(), SesTestSetup.SenderAddress, "shop.example.com"), Ct));

        Assert.Single(wire.Requests);
    }

    [Fact]
    public async Task Client_ConnectionThatBreaks_IsNotRepeated()
    {
        using SesWire wire = new(_ => new WireAnswer.Refuse());
        using AmazonSimpleEmailServiceV2Client client = SesTestSetup.Client(wire);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            client.SendEmailAsync(SesEmailGateway.Compose(Message(), SesTestSetup.SenderAddress, "shop.example.com"), Ct));

        Assert.Single(wire.Requests);
    }

    // ---- the pure mapping: 200 is Sent, every other answer is a channel error ----

    [Fact]
    public void Classify_NoException_IsSent()
    {
        Assert.Equal(new EmailSendResult.Sent(), SesErrorMapping.Classify(null));
    }

    public static TheoryData<Exception, string> TableExceptions => new()
    {
        { new MessageRejectedException("Email address is not verified. The following identities failed the check: x@y.example"), SesErrorMapping.IdentityNotVerifiedReason },
        { new MessageRejectedException("EMAIL ADDRESS IS NOT VERIFIED"), SesErrorMapping.IdentityNotVerifiedReason },
        { new MessageRejectedException("Illegal address"), "MessageRejected" },
        { new MessageRejectedException(string.Empty), "MessageRejected" },
        { new BadRequestException("invalid"), "BadRequest" },
        { new AccountSuspendedException("suspended"), "AccountSuspended" },
        { new SendingPausedException("paused"), "SendingPaused" },
        { new MailFromDomainNotVerifiedException("mx"), "MailFromDomainNotVerified" },
        { new TooManyRequestsException("slow down"), "TooManyRequests" },
        { new LimitExceededException("limit"), "LimitExceeded" },
        { new NotFoundException("missing"), "NotFound" },
        { new AmazonServiceException("Daily message quota exceeded") { ErrorCode = "ThrottlingException", StatusCode = HttpStatusCode.BadRequest }, "ThrottlingException" },
        { new AmazonServiceException("signature") { ErrorCode = "InvalidSignatureException", StatusCode = HttpStatusCode.Forbidden }, "InvalidSignatureException" },
        { new AmazonServiceException("denied") { ErrorCode = "AccessDeniedException", StatusCode = HttpStatusCode.Forbidden }, "AccessDeniedException" },
        { new AmazonServiceException("boom") { StatusCode = HttpStatusCode.BadGateway }, SesErrorMapping.StatusReasonPrefix + "502" },
        { new AmazonServiceException("code with a value alice@example.com") { ErrorCode = "code with a value alice@example.com", StatusCode = HttpStatusCode.BadRequest }, SesErrorMapping.StatusReasonPrefix + "400" },
        { new AmazonClientException("no route"), SesErrorMapping.ClientReason },
        { new HttpRequestException(HttpRequestError.ConnectionError, "refused"), "connection:ConnectionError" },
        { new TimeoutException("slow"), ExternalCallFailure.TimeoutReason },
        { new OperationCanceledException(), ExternalCallFailure.TimeoutReason },
        { new InvalidOperationException("not the library's"), SesErrorMapping.UnexpectedReasonPrefix + "InvalidOperationException" },
    };

    [Theory]
    [MemberData(nameof(TableExceptions))]
    public void Classify_EveryOtherAnswer_IsAChannelError_WithTheCodeAndNeverTheMessage(Exception answer, string reason)
    {
        EmailSendResult result = SesErrorMapping.Classify(answer);

        Assert.Equal(new EmailSendResult.ChannelError(reason), result);
        EmailSendResult.ChannelError error = Assert.IsType<EmailSendResult.ChannelError>(result);
        Assert.DoesNotContain("alice@example.com", error.ReasonCode, StringComparison.Ordinal);
        Assert.DoesNotContain("x@y.example", error.ReasonCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Classify_IsPure_TheSameAnswerGivesTheSameResult()
    {
        MessageRejectedException answer = new("Email address is not verified");

        Assert.Equal(SesErrorMapping.Classify(answer), SesErrorMapping.Classify(answer));
        Assert.Equal(SesErrorMapping.FailureOf(answer), SesErrorMapping.FailureOf(answer));
    }

    public static TheoryData<Exception, ExternalCallFailureKind> Kinds => new()
    {
        { new TimeoutException("x"), ExternalCallFailureKind.TimedOut },
        { new OperationCanceledException(), ExternalCallFailureKind.TimedOut },
        { new AmazonClientException("x"), ExternalCallFailureKind.Unreachable },
        { new HttpRequestException(HttpRequestError.ConnectionError, "x"), ExternalCallFailureKind.Unreachable },
        { new MessageRejectedException("x"), ExternalCallFailureKind.Unrecognized },
        { new InvalidOperationException("x"), ExternalCallFailureKind.Unrecognized },
    };

    [Theory]
    [MemberData(nameof(Kinds))]
    public void FailureOf_GivesTheKindTheLogCarries(Exception answer, ExternalCallFailureKind kind)
    {
        Assert.Equal(kind, SesErrorMapping.FailureOf(answer).Kind);
    }

    [Fact]
    public void Classify_ANotFoundOfTheSend_IsTheTablesRow()
    {
        // The pre-query's 404 is handled before the mapping is reached (08 2.2.2); a NotFoundException the send itself gets is the table's NotFoundException row.
        Assert.Equal(new EmailSendResult.ChannelError("NotFound"), SesErrorMapping.Classify(new NotFoundException("resource")));
    }
}
