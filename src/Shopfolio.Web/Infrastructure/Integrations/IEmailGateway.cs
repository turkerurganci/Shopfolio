namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The e-mail sending service behind the link layer (08 3.1 rule 1, 2.2; 05 3.6.1; MK-17; 09 11.1 item 1). Both jobs leave the job queue
/// (30 seconds), are not retried by the layer - the dispatch record's next attempt is the retry (05 4.4.3) - and return a class of their family.
/// In development the implementation writes to the capture tool's SMTP port (<see cref="SmtpCaptureEmailGateway"/>; 09 4.3 item 2).
/// </summary>
internal interface IEmailGateway
{
    /// <summary>Sends one message to one recipient; the suspended-recipient pre-query of the service is part of this job (09 11.2 item 4).</summary>
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct);

    /// <summary>Asks the permanently bounced addresses added since <paramref name="since"/> (08 2.2.3: the last 24 hours).</summary>
    Task<BounceQueryResult> QueryBouncesAsync(DateTimeOffset since, CancellationToken ct);
}

/// <summary>
/// One message to one recipient (08 2.2.2): the sender's display name (the brand name, or the domain while it is empty), the firm's address for
/// replies (none when empty), the subject and the two bodies, HTML and plain text, made from the same content. The sender address is the
/// installation setting and is added by the layer.
/// </summary>
internal sealed record EmailMessage(
    string RecipientEmail,
    string SenderDisplayName,
    string? ReplyToEmail,
    string Subject,
    string HtmlBody,
    string TextBody);
