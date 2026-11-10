using System.Net.Mail;
using System.Net.Mime;
using System.Text;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The development implementation of <see cref="IEmailGateway"/> (09 4.3 item 2, 19.1 item 7; K-1441): it writes each message to the capture
/// tool's SMTP port with the framework's SMTP client, so in development no e-mail leaves the machine and the developer reads it in the tool's
/// inbox. It keeps the layer's common face: one recipient per call, the job limit through <see cref="ExternalCallRunner"/>, no retry, and the
/// three classes - a refusal of the recipient's mailbox is a recipient error, every other SMTP failure, the timeout and the unreachable tool are
/// channel errors. The capture tool keeps no bounce list, so the bounce query is an empty list and makes no call. The automated tests do not use
/// it; they use the fake gateway.
/// </summary>
internal sealed class SmtpCaptureEmailGateway : IEmailGateway
{
    private readonly ISmtpTransport _transport;
    private readonly ExternalCallRunner _runner;
    private readonly IOptions<EmailOptions> _email;

    public SmtpCaptureEmailGateway(ISmtpTransport transport, ExternalCallRunner runner, IOptions<EmailOptions> email)
    {
        _transport = transport;
        _runner = runner;
        _email = email;
    }

    /// <summary>The SMTP replies that refuse the recipient's mailbox, not the channel (RFC 5321 450, 550, 551, 552, 553).</summary>
    public static readonly IReadOnlyList<SmtpStatusCode> RecipientStatuses =
    [
        SmtpStatusCode.MailboxBusy,
        SmtpStatusCode.MailboxUnavailable,
        SmtpStatusCode.UserNotLocalTryAlternatePath,
        SmtpStatusCode.ExceededStorageAllocation,
        SmtpStatusCode.MailboxNameNotAllowed,
    ];

    /// <summary>The reason code of a recipient address that is not an address.</summary>
    public const string InvalidAddressReason = "address-format";

    /// <summary>The reason code of an SMTP reply: its number, never the reply's text (which may repeat the address).</summary>
    public static string ReasonOf(SmtpStatusCode status) => "smtp:" + (int)status;

    /// <summary>
    /// The class of an SMTP failure that is not a refusal of the recipient: the client's own failure without a reply (no connection, the
    /// connection broke off) is an unreachable tool, any other reply is an answer the gateway does not list. Both are channel errors.
    /// </summary>
    public static ExternalCallFailure FailureOf(SmtpStatusCode status) =>
        status == SmtpStatusCode.GeneralFailure
            ? ExternalCallFailure.Unreachable(ReasonOf(status))
            : ExternalCallFailure.Unrecognized(ReasonOf(status));

    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _runner.RunAsync(
            IntegrationOperation.EmailSend,
            async token =>
            {
                if (!MailAddress.TryCreate(message.RecipientEmail, out _))
                {
                    // The address is the recipient's, not the channel's: no call is made.
                    return new EmailSendResult.RecipientError(InvalidAddressReason);
                }

                using MailMessage mail = Compose(message, _email.Value.SenderAddress);
                try
                {
                    await _transport.SendAsync(mail, token);
                    return new EmailSendResult.Sent();
                }
                catch (SmtpFailedRecipientException exception) when (RecipientStatuses.Contains(exception.StatusCode))
                {
                    return new EmailSendResult.RecipientError(ReasonOf(exception.StatusCode));
                }
                catch (SmtpException exception)
                {
                    // A client that gave up because the limit passed is the runner's timeout, not an SMTP reply.
                    token.ThrowIfCancellationRequested();
                    return _runner.Failed<EmailSendResult>(IntegrationOperation.EmailSend, FailureOf(exception.StatusCode));
                }
            },
            ct);
    }

    public Task<BounceQueryResult> QueryBouncesAsync(DateTimeOffset since, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult<BounceQueryResult>(new BounceQueryResult.List([]));
    }

    /// <summary>The message as it goes: one recipient, the display name with the sender address, the reply address when there is one, UTF-8, plain text with an HTML alternative.</summary>
    public static MailMessage Compose(EmailMessage message, string senderAddress)
    {
        ArgumentNullException.ThrowIfNull(message);
        MailMessage mail = new()
        {
            From = new MailAddress(senderAddress, message.SenderDisplayName, Encoding.UTF8),
            Subject = message.Subject,
            SubjectEncoding = Encoding.UTF8,
            Body = message.TextBody,
            BodyEncoding = Encoding.UTF8,
            HeadersEncoding = Encoding.UTF8,
            IsBodyHtml = false,
        };
        mail.To.Add(new MailAddress(message.RecipientEmail));
        // A reply address that is not an address is left out (D-106): the firm's setting is checked at its entry, and no exception leaves the layer.
        if (!string.IsNullOrWhiteSpace(message.ReplyToEmail) && MailAddress.TryCreate(message.ReplyToEmail, out MailAddress? replyTo))
        {
            mail.ReplyToList.Add(replyTo);
        }

        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.HtmlBody, Encoding.UTF8, MediaTypeNames.Text.Html));
        return mail;
    }
}

/// <summary>The SMTP sending step of <see cref="SmtpCaptureEmailGateway"/>; the tests give a fake one, so they make no network call.</summary>
internal interface ISmtpTransport
{
    Task SendAsync(MailMessage message, CancellationToken ct);
}

/// <summary>Sends over the framework's SMTP client to the capture tool's host and port (no TLS, no credential: the tool is inside the Compose network).</summary>
internal sealed class SmtpTransport : ISmtpTransport
{
    private readonly IOptions<EmailCaptureOptions> _options;

    public SmtpTransport(IOptions<EmailCaptureOptions> options)
    {
        _options = options;
    }

    public async Task SendAsync(MailMessage message, CancellationToken ct)
    {
        EmailCaptureOptions options = _options.Value;
        using SmtpClient client = new(options.Host, options.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = false,
            UseDefaultCredentials = false,
        };
        await client.SendMailAsync(message, ct);
    }
}
