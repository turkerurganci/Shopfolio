using System.Net.Mail;
using System.Net.Mime;
using System.Text.Json;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The development e-mail gateway (T202 criteria 3, 5, 6, 7, 8 and 9, KK3, KK5, KK6, KK7, KK9; 09 4.3 item 2, 19.1 item 7): the message as it
/// goes, the recipient's refusal against the channel's failure, the job limit on the fake clock, and a failure's log line without the address
/// or the subject. The SMTP step is a hand-written transport here, so no test opens a connection.
/// </summary>
public sealed class SmtpCaptureEmailGatewayTests
{
    private const string SenderAddress = "dev@example.com";
    private const string Recipient = "recipient-6620@example.com";
    private const string Subject = "Siparişiniz alındı SUBJECT-3318";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static EmailMessage Message(string recipient = Recipient, string? replyTo = "info@example.com") =>
        new(recipient, "Örnek Mağaza", replyTo, Subject, "<p>Merhaba</p>", "Merhaba");

    private static SmtpCaptureEmailGateway Gateway(RunnerHarness harness, ISmtpTransport transport) =>
        new(transport, harness.Runner, Options.Create(new EmailOptions { SenderAddress = SenderAddress }));

    public static TheoryData<SmtpStatusCode> RecipientStatuses => [.. SmtpCaptureEmailGateway.RecipientStatuses];

    [Fact]
    public async Task SendAsync_AcceptedByTheTool_IsSent_WithOneRecipientTheSenderTheReplyAddressAndBothBodies()
    {
        using RunnerHarness harness = new();
        RecordingTransport transport = new();

        EmailSendResult result = await Gateway(harness, transport).SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.Sent(), result);
        SentMail mail = Assert.Single(transport.Sent);
        Assert.Equal([Recipient], mail.To);
        Assert.Equal(SenderAddress, mail.From);
        Assert.Equal("Örnek Mağaza", mail.FromName);
        Assert.Equal(["info@example.com"], mail.ReplyTo);
        Assert.Equal(Subject, mail.Subject);
        Assert.Equal("Merhaba", mail.TextBody);
        Assert.Equal([MediaTypeNames.Text.Html], mail.AlternateTypes);
        Assert.Empty(harness.Sink.Events);
    }

    [Fact]
    public async Task SendAsync_WithoutAReplyAddress_AddsNone()
    {
        using RunnerHarness harness = new();
        RecordingTransport transport = new();

        await Gateway(harness, transport).SendAsync(Message(replyTo: null), Ct);

        Assert.Empty(Assert.Single(transport.Sent).ReplyTo);
    }

    [Theory]
    [MemberData(nameof(RecipientStatuses))]
    public async Task SendAsync_RecipientRefused_IsARecipientError_WithTheReplyNumberOnly(SmtpStatusCode status)
    {
        using RunnerHarness harness = new();
        RecordingTransport transport = new() { Failure = new SmtpFailedRecipientException(status, Recipient) };

        EmailSendResult result = await Gateway(harness, transport).SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.RecipientError(SmtpCaptureEmailGateway.ReasonOf(status)), result);
        Assert.Equal(1, transport.Calls);
    }

    [Theory]
    [InlineData(SmtpStatusCode.GeneralFailure, ExternalCallFailureKind.Unreachable)]
    [InlineData(SmtpStatusCode.TransactionFailed, ExternalCallFailureKind.Unrecognized)]
    [InlineData(SmtpStatusCode.ServiceNotAvailable, ExternalCallFailureKind.Unrecognized)]
    public async Task SendAsync_OtherSmtpFailure_IsAChannelError_AndItsLineHasNoAddressNoSubjectNoReplyText(SmtpStatusCode status, ExternalCallFailureKind kind)
    {
        using RunnerHarness harness = new();
        RecordingTransport transport = new() { Failure = new SmtpException(status, "5.0.0 refused for " + Recipient + " " + Subject) };

        EmailSendResult result = await Gateway(harness, transport).SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.ChannelError(SmtpCaptureEmailGateway.ReasonOf(status)), result);
        Assert.Equal(1, transport.Calls);
        string line = Assert.Single(harness.Lines);
        Assert.DoesNotContain(Recipient, line, StringComparison.Ordinal);
        Assert.DoesNotContain("SUBJECT-3318", line, StringComparison.Ordinal);
        Assert.DoesNotContain(SenderAddress, line, StringComparison.Ordinal);
        Assert.DoesNotContain("refused for", line, StringComparison.Ordinal);
        using JsonDocument document = JsonDocument.Parse(line);
        Assert.Equal(LogEvents.ExternalCallFailed, document.RootElement.GetProperty("event").GetString());
        Assert.Equal(kind.ToString(), document.RootElement.GetProperty("failureKind").GetString());
        Assert.Equal(SmtpCaptureEmailGateway.ReasonOf(status), document.RootElement.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task SendAsync_RecipientExceptionWithAChannelReply_IsAChannelError()
    {
        using RunnerHarness harness = new();
        RecordingTransport transport = new() { Failure = new SmtpFailedRecipientException(SmtpStatusCode.TransactionFailed, Recipient) };

        EmailSendResult result = await Gateway(harness, transport).SendAsync(Message(), Ct);

        Assert.Equal(new EmailSendResult.ChannelError(SmtpCaptureEmailGateway.ReasonOf(SmtpStatusCode.TransactionFailed)), result);
    }

    [Fact]
    public async Task SendAsync_ToolThatHangs_EndsAtTheJobLimitAsAChannelError_AndGoesOnce()
    {
        using RunnerHarness harness = new();
        RecordingTransport transport = new() { Hang = true };

        Task<EmailSendResult> call = Gateway(harness, transport).SendAsync(Message(), Ct);
        harness.Time.Advance(ExternalCallTimeouts.Job - TimeSpan.FromTicks(1));
        Assert.False(call.IsCompleted);
        harness.Time.Advance(TimeSpan.FromTicks(1));
        EmailSendResult result = await call;

        Assert.Equal(new EmailSendResult.ChannelError(ExternalCallFailure.TimeoutReason), result);
        Assert.Equal(1, transport.Calls);
        using JsonDocument document = JsonDocument.Parse(Assert.Single(harness.Lines));
        Assert.Equal(nameof(ExternalCallFailureKind.TimedOut), document.RootElement.GetProperty("failureKind").GetString());
    }

    [Theory]
    [InlineData("not-an-address")]
    [InlineData("")]
    public async Task SendAsync_RecipientThatIsNoAddress_IsARecipientError_AndMakesNoCall(string recipient)
    {
        using RunnerHarness harness = new();
        RecordingTransport transport = new();

        EmailSendResult result = await Gateway(harness, transport).SendAsync(Message(recipient), Ct);

        Assert.Equal(new EmailSendResult.RecipientError(SmtpCaptureEmailGateway.InvalidAddressReason), result);
        Assert.Equal(0, transport.Calls);
    }

    [Fact]
    public async Task QueryBouncesAsync_IsAnEmptyList_AndMakesNoCall()
    {
        using RunnerHarness harness = new();
        RecordingTransport transport = new();

        BounceQueryResult result = await Gateway(harness, transport).QueryBouncesAsync(RunnerHarness.Now, Ct);

        BounceQueryResult.List list = Assert.IsType<BounceQueryResult.List>(result);
        Assert.Empty(list.Addresses);
        Assert.Equal(0, transport.Calls);
    }

    private sealed record SentMail(
        IReadOnlyList<string> To, string From, string FromName, IReadOnlyList<string> ReplyTo, string Subject, string TextBody, IReadOnlyList<string> AlternateTypes);

    /// <summary>Keeps what each message carried, or fails as told, or hangs until its token is cancelled.</summary>
    private sealed class RecordingTransport : ISmtpTransport
    {
        private int _calls;

        public Exception? Failure { get; init; }

        public bool Hang { get; init; }

        public List<SentMail> Sent { get; } = [];

        public int Calls => Volatile.Read(ref _calls);

        public async Task SendAsync(MailMessage message, CancellationToken ct)
        {
            Interlocked.Increment(ref _calls);
            if (Hang)
            {
                TaskCompletionSource never = new(TaskCreationOptions.RunContinuationsAsynchronously);
                await using (ct.Register(() => never.TrySetCanceled(ct)))
                {
                    await never.Task;
                }
            }

            if (Failure is not null)
            {
                throw Failure;
            }

            Sent.Add(new SentMail(
                [.. message.To.Select(a => a.Address)],
                message.From!.Address,
                message.From.DisplayName,
                [.. message.ReplyToList.Select(a => a.Address)],
                message.Subject,
                message.Body,
                [.. message.AlternateViews.Select(v => v.ContentType.MediaType)]));
        }
    }
}
