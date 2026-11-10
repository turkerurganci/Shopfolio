using System.Net;
using System.Text.Json;

using Amazon.SimpleEmailV2;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The e-mail service's gateway on the real library client over a scripted wire (T407 criteria 1, 4, 5, 6, 7, KK1, KK4, KK5, KK6, KK7; 08 2.2.2; 09 11.2 items 4 and 6): the
/// request as it goes (one recipient and none of the fields the table does not send), the sender text and the two empty-value arms, the three arms of the recipient
/// pre-query and the pre-query's failures that never stop the send, every row of the error table, and the failure's log line without the address, the key,
/// the subject or the service's message. No test opens a socket; the clock is the fake one.
/// </summary>
public sealed class SesEmailGatewayTests
{
    private const string Domain = "shop.example.com";
    private const string Recipient = "Musteri.Ornek+Etiket@Example.COM";
    private const string Subject = "Örnek Mağaza: Siparişiniz alındı SUBJECT-5521";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static EmailMessage Message(string recipient = Recipient, string name = "Örnek Mağaza", string? replyTo = "info@firma.example") =>
        new(recipient, name, replyTo, Subject, "<p>Merhaba Çağrı</p>", "Merhaba Çağrı");

    /// <summary>The wire, the client, the runner on the fake clock and the gateway, made the way the application makes them.</summary>
    private sealed class Rig : IDisposable
    {
        private readonly RunnerHarness _harness = new();

        public Rig(Func<WireRequest, WireAnswer> script)
        {
            Wire = new SesWire(script);
            Client = SesTestSetup.Client(Wire);
            Gateway = new SesEmailGateway(Client, _harness.Runner, Options.Create(SesTestSetup.Options()), Options.Create(new SiteOptions { Domain = Domain }));
        }

        public SesWire Wire { get; }

        public AmazonSimpleEmailServiceV2Client Client { get; }

        public SesEmailGateway Gateway { get; }

        public RunnerHarness Harness => _harness;

        public IReadOnlyList<WireRequest> Sends => [.. Wire.Requests.Where(r => r.IsSend)];

        public IReadOnlyList<WireRequest> Queries => [.. Wire.Requests.Where(r => r.IsQuery)];

        public void Dispose()
        {
            Client.Dispose();
            Wire.Dispose();
            _harness.Dispose();
        }
    }

    private static WireAnswer Answer(WireRequest request, WireAnswer onQuery, WireAnswer onSend) => request.IsQuery ? onQuery : onSend;

    private static Func<WireRequest, WireAnswer> Script(WireAnswer onQuery, WireAnswer onSend) => request => Answer(request, onQuery, onSend);

    // ---- KK1: one recipient, none of the forbidden fields ----

    [Fact]
    public async Task SendAsync_SendsOneCallToOneRecipient_WithNoCcNoBccAndNoneOfTheSixFieldsOfTheNotSentRow()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.Sent(), result);
        WireRequest send = Assert.Single(rig.Sends);
        Assert.Equal("email.eu-central-1.amazonaws.com", send.Uri.Host);
        using JsonDocument body = send.Json();
        JsonElement root = body.RootElement;
        Assert.Equal([Recipient], root.GetProperty("Destination").GetProperty("ToAddresses").EnumerateArray().Select(e => e.GetString()));
        Assert.False(root.GetProperty("Destination").TryGetProperty("CcAddresses", out _));
        Assert.False(root.GetProperty("Destination").TryGetProperty("BccAddresses", out _));
        foreach (string notSent in new[] { "ConfigurationSetName", "ConfigurationOverrides", "EmailTags", "ListManagementOptions", "Attachments" })
        {
            Assert.False(root.TryGetProperty(notSent, out _), notSent + " must not be sent");
        }

        Assert.False(root.GetProperty("Content").GetProperty("Simple").TryGetProperty("Headers", out _));
        Assert.Equal(["Content", "Destination", "FromEmailAddress", "ReplyToAddresses"], root.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task SendAsync_TwoMessages_AreTwoCallsEachToItsOwnRecipient()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        await rig.Gateway.SendAsync(Message("bir@example.com"), Ct);
        await rig.Gateway.SendAsync(Message("iki@example.com"), Ct);

        Assert.Equal(2, rig.Sends.Count);
        using JsonDocument second = rig.Sends[1].Json();
        Assert.Equal(["iki@example.com"], second.RootElement.GetProperty("Destination").GetProperty("ToAddresses").EnumerateArray().Select(e => e.GetString()));
    }

    // ---- KK4: the sender text, the reply address, both bodies in UTF-8 ----

    [Fact]
    public async Task SendAsync_PutsTheEscapedBrandNameAndTheSenderAddressInFrom_TheFirmsAddressInReplyTo_AndBothBodiesInUtf8()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        await rig.Gateway.SendAsync(Message(name: "Ahmet \"Usta\" \\ Çiçekçi"), Ct);

        using JsonDocument body = Assert.Single(rig.Sends).Json();
        JsonElement root = body.RootElement;
        Assert.Equal("\"Ahmet \\\"Usta\\\" \\\\ Çiçekçi\" <" + SesTestSetup.SenderAddress + ">", root.GetProperty("FromEmailAddress").GetString());
        Assert.Equal(["info@firma.example"], root.GetProperty("ReplyToAddresses").EnumerateArray().Select(e => e.GetString()));
        JsonElement simple = root.GetProperty("Content").GetProperty("Simple");
        Assert.Equal(Subject, simple.GetProperty("Subject").GetProperty("Data").GetString());
        Assert.Equal("UTF-8", simple.GetProperty("Subject").GetProperty("Charset").GetString());
        Assert.Equal("<p>Merhaba Çağrı</p>", simple.GetProperty("Body").GetProperty("Html").GetProperty("Data").GetString());
        Assert.Equal("UTF-8", simple.GetProperty("Body").GetProperty("Html").GetProperty("Charset").GetString());
        Assert.Equal("Merhaba Çağrı", simple.GetProperty("Body").GetProperty("Text").GetProperty("Data").GetString());
        Assert.Equal("UTF-8", simple.GetProperty("Body").GetProperty("Text").GetProperty("Charset").GetString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendAsync_WithoutAReplyAddress_SendsNoReplyToField(string? replyTo)
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        await rig.Gateway.SendAsync(Message(replyTo: replyTo), Ct);

        using JsonDocument body = Assert.Single(rig.Sends).Json();
        Assert.False(body.RootElement.TryGetProperty("ReplyToAddresses", out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SendAsync_WithAnEmptyBrandName_PutsTheDomainInFrom(string name)
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        await rig.Gateway.SendAsync(Message(name: name), Ct);

        using JsonDocument body = Assert.Single(rig.Sends).Json();
        Assert.Equal("\"" + Domain + "\" <" + SesTestSetup.SenderAddress + ">", body.RootElement.GetProperty("FromEmailAddress").GetString());
    }

    [Theory]
    [InlineData("Örnek Mağaza", "\"Örnek Mağaza\" <a@b.example>")]
    [InlineData("A\"B", "\"A\\\"B\" <a@b.example>")]
    [InlineData("A\\B", "\"A\\\\B\" <a@b.example>")]
    [InlineData("\\\"", "\"\\\\\\\"\" <a@b.example>")]
    [InlineData("Satır\r\nSonu", "\"Satır  Sonu\" <a@b.example>")]
    [InlineData("Sekme\tve\u0007zil\u0085nel", "\"Sekme ve zil nel\" <a@b.example>")]
    [InlineData("Ay\u00e7\u2028\u2029sonu", "\"Ay\u00e7  sonu\" <a@b.example>")]
    public void SenderText_EscapesTheQuoteAndTheBackslash_AndTurnsLineBreaksAndControlCharactersIntoBlanks(string name, string expected)
    {
        Assert.Equal(expected, SesEmailGateway.SenderText(name, "a@b.example", Domain));
    }

    [Fact]
    public void SenderText_WithNoName_IsTheDomain()
    {
        Assert.Equal("\"" + Domain + "\" <a@b.example>", SesEmailGateway.SenderText(null, "a@b.example", Domain));
    }

    // ---- KK5: the pre-query ----

    [Fact]
    public async Task SendAsync_RecipientOnTheListAsABounce_IsARecipientError_AndMakesNoSendCall()
    {
        using Rig rig = new(Script(WireAnswer.Listed(Recipient), WireAnswer.Sent()));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.RecipientError(SesEmailGateway.SuppressedReason), result);
        Assert.Single(rig.Queries);
        Assert.Empty(rig.Sends);
        Assert.Empty(rig.Harness.Lines);
    }

    [Fact]
    public async Task SendAsync_RecipientNotOnTheList_404_Sends_AndTheQuerys404IsNotATableRow()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.Sent(), result);
        Assert.Single(rig.Queries);
        Assert.Single(rig.Sends);
        Assert.Empty(rig.Harness.Lines);
    }

    [Fact]
    public async Task SendAsync_AsksTheRecipientAsItWasEntered_WithoutTrimmingOrChangingCase()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        await rig.Gateway.SendAsync(Message(), Ct);

        WireRequest query = Assert.Single(rig.Queries);
        Assert.Equal("/v2/email/suppression/addresses/" + Recipient, Uri.UnescapeDataString(query.Uri.AbsolutePath));
        using JsonDocument send = Assert.Single(rig.Sends).Json();
        Assert.Equal(Recipient, send.RootElement.GetProperty("Destination").GetProperty("ToAddresses")[0].GetString());
    }

    [Fact]
    public async Task SendAsync_RecipientOnTheListForAnotherReason_IsNotAStop_TheSendGoes()
    {
        using Rig rig = new(Script(WireAnswer.Listed(Recipient, "COMPLAINT"), WireAnswer.Sent()));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.Sent(), result);
        Assert.Single(rig.Sends);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, "TooManyRequestsException")]
    [InlineData(HttpStatusCode.Forbidden, "AccessDeniedException")]
    [InlineData(HttpStatusCode.Forbidden, "UnrecognizedClientException")]
    [InlineData(HttpStatusCode.BadRequest, "BadRequestException")]
    [InlineData(HttpStatusCode.InternalServerError, "InternalFailure")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "ServiceUnavailable")]
    public async Task SendAsync_PreQueryAnswersAnErrorThatIsNot404_DoesNotStopTheSend_AndTheResultIsTheSends(HttpStatusCode status, string errorType)
    {
        using Rig rig = new(Script(WireAnswer.Error(status, errorType, "query failed for " + Recipient), WireAnswer.Sent()));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.Sent(), result);
        Assert.Single(rig.Queries);
        Assert.Single(rig.Sends);
    }

    [Fact]
    public async Task SendAsync_PreQueryFailsAndTheSendFails_TheChannelErrorIsTheSends_NotTheQuerys()
    {
        using Rig rig = new(Script(
            WireAnswer.Error(HttpStatusCode.Forbidden, "AccessDeniedException", "no permission"),
            WireAnswer.Error(HttpStatusCode.BadRequest, "BadRequestException", "bad")));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.ChannelError("BadRequestException"), result);
    }

    [Fact]
    public async Task SendAsync_PreQueryFailureIsInTheLogOnlyWithItsOwnPrefix_NotInTheResult()
    {
        using Rig rig = new(Script(WireAnswer.Error(HttpStatusCode.Forbidden, "AccessDeniedException", "no permission"), WireAnswer.Sent()));

        await rig.Gateway.SendAsync(Message(), Ct);

        using JsonDocument line = JsonDocument.Parse(Assert.Single(rig.Harness.Lines));
        Assert.Equal(SesEmailGateway.QueryReasonPrefix + "AccessDeniedException", line.RootElement.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task SendAsync_PreQueryThatHangs_EndsAtItsOwnJobLimit_AndTheSendStillGoesWithItsOwnLimit()
    {
        using Rig rig = new(Script(new WireAnswer.Hang(), WireAnswer.Sent()));

        Task<EmailSendResult> call = rig.Gateway.SendAsync(Message(), Ct);
        await rig.Wire.NextRequestAsync(Ct);
        rig.Harness.Time.Advance(ExternalCallTimeouts.Job);
        EmailSendResult result = await call;

        Assert.Equal(new EmailSendResult.Sent(), result);
        Assert.Single(rig.Queries);
        Assert.Single(rig.Sends);
    }

    // ---- KK2 and KK6: the limit, no retry, the error table ----

    [Fact]
    public async Task SendAsync_SendThatHangs_EndsAtTheJobLimitAsAChannelError_AndGoesOnce()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), new WireAnswer.Hang()));

        Task<EmailSendResult> call = rig.Gateway.SendAsync(Message(), Ct);
        await rig.Wire.NextRequestAsync(Ct);
        await rig.Wire.NextRequestAsync(Ct);
        rig.Harness.Time.Advance(ExternalCallTimeouts.Job - TimeSpan.FromTicks(1));
        Assert.False(call.IsCompleted);
        rig.Harness.Time.Advance(TimeSpan.FromTicks(1));
        EmailSendResult result = await call;

        Assert.Equal(new EmailSendResult.ChannelError(ExternalCallFailure.TimeoutReason), result);
        Assert.Single(rig.Sends);
        using JsonDocument line = JsonDocument.Parse(Assert.Single(rig.Harness.Lines));
        Assert.Equal(nameof(ExternalCallFailureKind.TimedOut), line.RootElement.GetProperty("failureKind").GetString());
    }

    [Fact]
    public async Task SendAsync_ConnectionRefused_IsAChannelError_AndTheSendIsNotRepeated()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), new WireAnswer.Refuse()));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        EmailSendResult.ChannelError error = Assert.IsType<EmailSendResult.ChannelError>(result);
        Assert.False(string.IsNullOrWhiteSpace(error.ReasonCode));
        Assert.Single(rig.Sends);
    }

    /// <summary>Every row of the table of 08 2.2.2, as the service answers it: the status, the error type, the message and the class with the reason code the log carries.</summary>
    public static TheoryData<HttpStatusCode, string, string, string> TableRows => new()
    {
        { HttpStatusCode.BadRequest, "MessageRejected", "Email address is not verified. The following identities failed the check in region EU-CENTRAL-1: bilgi@example.com", SesErrorMapping.IdentityNotVerifiedReason },
        { HttpStatusCode.BadRequest, "MessageRejected", "Illegal address. The message could not be sent.", "MessageRejected" },
        { HttpStatusCode.BadRequest, "MessageRejected", "Message content is invalid", "MessageRejected" },
        { HttpStatusCode.BadRequest, "BadRequestException", "The input is invalid", "BadRequestException" },
        { HttpStatusCode.BadRequest, "AccountSuspendedException", "The account is suspended", "AccountSuspendedException" },
        { HttpStatusCode.BadRequest, "SendingPausedException", "Sending is paused", "SendingPausedException" },
        { HttpStatusCode.BadRequest, "MailFromDomainNotVerifiedException", "The MX record cannot be read", "MailFromDomainNotVerifiedException" },
        { HttpStatusCode.TooManyRequests, "TooManyRequestsException", "Too many requests", "TooManyRequestsException" },
        { HttpStatusCode.BadRequest, "LimitExceededException", "Limit exceeded", "LimitExceededException" },
        { HttpStatusCode.BadRequest, "ThrottlingException", "Daily message quota exceeded", "ThrottlingException" },
        { HttpStatusCode.BadRequest, "ThrottlingException", "Maximum sending rate exceeded", "ThrottlingException" },
        { HttpStatusCode.NotFound, "NotFoundException", "The resource was not found", "NotFoundException" },
        { HttpStatusCode.Forbidden, "AccessDeniedException", "User is not authorized to perform ses:SendEmail", "AccessDeniedException" },
        { HttpStatusCode.Forbidden, "UnrecognizedClientException", "The security token included in the request is invalid", "UnrecognizedClientException" },
        { HttpStatusCode.Forbidden, "InvalidSignatureException", "The request signature does not match", "InvalidSignatureException" },
        { HttpStatusCode.InternalServerError, "InternalFailure", "Internal failure", "InternalFailure" },
        { HttpStatusCode.ServiceUnavailable, "ServiceUnavailable", "Service unavailable", "ServiceUnavailable" },
        { (HttpStatusCode)418, "SomethingNewException", "an answer the table does not list", "SomethingNewException" },
    };

    [Theory]
    [MemberData(nameof(TableRows))]
    public async Task SendAsync_EveryAnswerOfTheTable_IsAChannelError_SentOnce_WithTheReasonCodeAndNoMessageInTheLog(HttpStatusCode status, string errorType, string message, string reason)
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Error(status, errorType, message + " " + Recipient + " " + SesTestSetup.AccessKeyId)));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.ChannelError(reason), result);
        Assert.Single(rig.Sends);
        string line = Assert.Single(rig.Harness.Lines);
        using JsonDocument document = JsonDocument.Parse(line);
        Assert.Equal(LogEvents.ExternalCallFailed, document.RootElement.GetProperty("event").GetString());
        Assert.Equal(reason, document.RootElement.GetProperty("reason").GetString());
        AssertNoValueInTheLine(line);
    }

    [Fact]
    public async Task SendAsync_AnAnswerWithNoErrorTypeAtAll_IsStillAChannelError_WithTheStatusAsItsReason()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), new WireAnswer.Reply(HttpStatusCode.BadGateway, string.Empty)));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        EmailSendResult.ChannelError error = Assert.IsType<EmailSendResult.ChannelError>(result);
        Assert.False(string.IsNullOrWhiteSpace(error.ReasonCode));
        Assert.Single(rig.Sends);
    }

    // ---- KK7: nothing of the answer is kept ----

    [Fact]
    public async Task SendAsync_Success_ReturnsNothingOfTheAnswer_AndWritesNoLine()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.Sent(), result);
        Assert.DoesNotContain("SECRETID", result.ToString(), StringComparison.Ordinal);
        Assert.Empty(rig.Harness.Lines);
    }

    [Fact]
    public async Task SendAsync_RecipientError_WritesNoLine_AndKeepsNoAddress()
    {
        using Rig rig = new(Script(WireAnswer.Listed(Recipient), WireAnswer.Sent()));

        EmailSendResult result = await rig.Gateway.SendAsync(Message(), Ct);

        Assert.DoesNotContain(Recipient, result.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Empty(rig.Harness.Lines);
    }

    private static void AssertNoValueInTheLine(string line)
    {
        foreach (string value in new[] { Recipient, Recipient.ToLowerInvariant(), SesTestSetup.AccessKeyId, SesTestSetup.SecretKey, SesTestSetup.SenderAddress, "SUBJECT-5521", "Merhaba", "MessageId", "SECRETID", "info@firma.example" })
        {
            Assert.DoesNotContain(value, line, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ---- the bounce query is T408's ----

    [Fact]
    public async Task QueryBouncesAsync_WhileTheQueryIsNotBuilt_IsAQueryError_NeverAnEmptyList_AndMakesNoCall()
    {
        using Rig rig = new(Script(WireAnswer.NotListed(), WireAnswer.Sent()));

        BounceQueryResult result = await rig.Gateway.QueryBouncesAsync(RunnerHarness.Now, Ct);

        Assert.Equal(new BounceQueryResult.QueryFailed(SesEmailGateway.BounceQueryNotBuiltReason), result);
        Assert.Empty(rig.Wire.Requests);
    }
}
