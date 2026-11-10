using System.Globalization;
using System.Text;

using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The e-mail service's implementation of <see cref="IEmailGateway"/> (08 2.2, 2.2.2; 09 11.1, 11.2): one SendEmail call per message, to one recipient,
/// through the one client of the application. Before each send the recipient is asked of the service's suppression list (GetSuppressedDestination) with the
/// address as it was entered: <c>200</c> with the reason <c>BOUNCE</c> is a recipient error and no send is made, <c>404</c> means the address is not listed and
/// the message goes, and any other answer - the throttling, a credential or permission error, the timeout - does not stop the send and is not written to the
/// channel state (it is written to the application log only). The answer of the send is mapped by <see cref="SesErrorMapping"/>. Both calls run through
/// <see cref="ExternalCallRunner"/> with their own job limit; neither is retried (the dispatch row's next attempt is the retry, 05 4.4.3). The service's
/// <c>MessageId</c> is not read or kept. The bounce query (08 2.2.3; T408) is one ListSuppressedDestinations read of the account-level list, with no deletion call.
/// </summary>
internal sealed class SesEmailGateway : IEmailGateway
{
    /// <summary>The character set of the subject and of both bodies (08 2.2.2).</summary>
    public const string Charset = "UTF-8";

    /// <summary>The reason code of a recipient that the service's list holds as a bounced address.</summary>
    public const string SuppressedReason = "suppressed-bounce";

    /// <summary>The prefix of the reason code of a failed pre-query, so the log tells it from the send's failure.</summary>
    public const string QueryReasonPrefix = "suppression-query:";

    /// <summary>The page size of the bounce query: the service's largest (1000), so a window is read in as few calls as the service allows.</summary>
    public const int QueryPageSize = 1000;

    private readonly IAmazonSimpleEmailServiceV2 _client;
    private readonly ExternalCallRunner _runner;
    private readonly IOptions<EmailOptions> _email;
    private readonly IOptions<SiteOptions> _site;

    public SesEmailGateway(IAmazonSimpleEmailServiceV2 client, ExternalCallRunner runner, IOptions<EmailOptions> email, IOptions<SiteOptions> site)
    {
        _client = client;
        _runner = runner;
        _email = email;
        _site = site;
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);
        RecipientQuery answer = await _runner.RunAsync(IntegrationOperation.EmailSend, token => AskRecipientAsync(message.RecipientEmail, token), ct);
        if (answer is RecipientQuery.Listed)
        {
            return new EmailSendResult.RecipientError(SuppressedReason);
        }

        SendEmailRequest request = Compose(message, _email.Value.SenderAddress, _site.Value.Domain);
        return await _runner.RunAsync(IntegrationOperation.EmailSend, token => SendCoreAsync(request, token), ct);
    }

    /// <summary>
    /// The permanently bounced addresses the service's account-level list took in since <paramref name="since"/> (08 2.2.3): one ListSuppressedDestinations request with
    /// <c>StartDate</c> and the reason <c>BOUNCE</c>, repeated with the page token until the service gives none. The whole read is one job of the layer (one limit, no retry);
    /// any page that fails ends it with a query error and nothing of the earlier pages is given - a half list is never the answer. A summary whose reason is not
    /// <c>BOUNCE</c> is not read, whatever the request asked. The addresses are returned as the list holds them.
    /// </summary>
    public Task<BounceQueryResult> QueryBouncesAsync(DateTimeOffset since, CancellationToken ct) =>
        _runner.RunAsync(IntegrationOperation.BounceQuery, token => ListBouncesAsync(since, token), ct);

    private async Task<BounceQueryResult> ListBouncesAsync(DateTimeOffset since, CancellationToken token)
    {
        try
        {
            List<string> addresses = [];
            string? nextPage = null;
            do
            {
                ListSuppressedDestinationsResponse page = await _client.ListSuppressedDestinationsAsync(
                    new ListSuppressedDestinationsRequest
                    {
                        StartDate = since.UtcDateTime,
                        Reasons = [SuppressionListReason.BOUNCE],
                        PageSize = QueryPageSize,
                        NextToken = nextPage,
                    },
                    token);
                foreach (SuppressedDestinationSummary summary in page.SuppressedDestinationSummaries ?? [])
                {
                    if (summary.Reason == SuppressionListReason.BOUNCE && !string.IsNullOrWhiteSpace(summary.EmailAddress))
                    {
                        addresses.Add(summary.EmailAddress);
                    }
                }

                nextPage = string.IsNullOrEmpty(page.NextToken) ? null : page.NextToken;
            }
            while (nextPage is not null);

            return new BounceQueryResult.List(addresses);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            return _runner.Failed<BounceQueryResult>(IntegrationOperation.BounceQuery, ExternalCallFailure.TimedOut());
        }
        catch (AmazonServiceException exception)
        {
            return _runner.Failed<BounceQueryResult>(IntegrationOperation.BounceQuery, SesErrorMapping.FailureOf(exception));
        }
        catch (AmazonClientException exception)
        {
            return _runner.Failed<BounceQueryResult>(IntegrationOperation.BounceQuery, SesErrorMapping.FailureOf(exception));
        }
    }

    /// <summary>
    /// The SendEmail request of one message (08 2.2.2 table): the sender with the escaped brand name, one recipient as entered, the firm's reply address when there is one,
    /// the subject and the HTML and plain bodies in UTF-8. The six fields of the "Gönderilmez" row, Cc and Bcc are never set.
    /// </summary>
    public static SendEmailRequest Compose(EmailMessage message, string senderAddress, string domain)
    {
        ArgumentNullException.ThrowIfNull(message);
        SendEmailRequest request = new()
        {
            FromEmailAddress = SenderText(message.SenderDisplayName, senderAddress, domain),
            Destination = new Destination { ToAddresses = [message.RecipientEmail] },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = message.Subject, Charset = Charset },
                    Body = new Body
                    {
                        Html = new Content { Data = message.HtmlBody, Charset = Charset },
                        Text = new Content { Data = message.TextBody, Charset = Charset },
                    },
                },
            },
        };

        if (!string.IsNullOrWhiteSpace(message.ReplyToEmail))
        {
            request.ReplyToAddresses = [message.ReplyToEmail];
        }

        return request;
    }

    /// <summary>
    /// The From text: <c>"{name}" &lt;{address}&gt;</c> - the brand name, or the domain while the name is empty; a backslash and a double quote of the name get a backslash,
    /// a line break and any other control character become a blank (header safety, 08 2.2.2).
    /// </summary>
    public static string SenderText(string? displayName, string senderAddress, string domain)
    {
        string name = string.IsNullOrWhiteSpace(displayName) ? domain : displayName;
        StringBuilder text = new(name.Length + senderAddress.Length + 5);
        text.Append('"');
        foreach (char c in name)
        {
            UnicodeCategory category = char.GetUnicodeCategory(c);
            if (category is UnicodeCategory.Control or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator)
            {
                text.Append(' ');
            }
            else
            {
                if (c is '"' or '\\')
                {
                    text.Append('\\');
                }

                text.Append(c);
            }
        }

        return text.Append("\" <").Append(senderAddress).Append('>').ToString();
    }

    private async Task<RecipientQuery> AskRecipientAsync(string recipient, CancellationToken token)
    {
        try
        {
            GetSuppressedDestinationResponse answer = await _client.GetSuppressedDestinationAsync(
                new GetSuppressedDestinationRequest { EmailAddress = recipient }, token);
            return answer.SuppressedDestination?.Reason == SuppressionListReason.BOUNCE ? new RecipientQuery.Listed() : new RecipientQuery.Proceed();
        }
        catch (NotFoundException)
        {
            // The address is not on the list: the send goes. This 404 is not the table's NotFoundException row (08 2.2.2).
            return new RecipientQuery.Proceed();
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            return QueryFailed(ExternalCallFailure.TimedOut());
        }
        catch (AmazonServiceException exception)
        {
            return QueryFailed(SesErrorMapping.FailureOf(exception));
        }
        catch (AmazonClientException exception)
        {
            return QueryFailed(SesErrorMapping.FailureOf(exception));
        }
    }

    private RecipientQuery QueryFailed(ExternalCallFailure failure) =>
        _runner.Failed<RecipientQuery>(IntegrationOperation.EmailSend, failure with { ReasonCode = QueryReasonPrefix + failure.ReasonCode });

    private async Task<EmailSendResult> SendCoreAsync(SendEmailRequest request, CancellationToken token)
    {
        try
        {
            // The service's MessageId is in the answer and is not read (08 2.2.2).
            _ = await _client.SendEmailAsync(request, token);
            return SesErrorMapping.Classify(null);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            // The library's own limit passed before the layer's: the same timeout result.
            return _runner.Failed<EmailSendResult>(IntegrationOperation.EmailSend, ExternalCallFailure.TimedOut());
        }
        catch (AmazonServiceException exception)
        {
            return _runner.Failed<EmailSendResult>(IntegrationOperation.EmailSend, SesErrorMapping.FailureOf(exception));
        }
        catch (AmazonClientException exception)
        {
            return _runner.Failed<EmailSendResult>(IntegrationOperation.EmailSend, SesErrorMapping.FailureOf(exception));
        }
    }

    /// <summary>The answer of the pre-query: the address is on the list as a bounce, or the send goes on (not listed, or no usable answer).</summary>
    private abstract record RecipientQuery : IExternalCallResult<RecipientQuery>
    {
        private RecipientQuery()
        {
        }

        public sealed record Listed : RecipientQuery;

        public sealed record Proceed : RecipientQuery;

        /// <summary>A pre-query without a usable answer never stops the send.</summary>
        public static RecipientQuery FromFailure(ExternalCallFailure failure) => new Proceed();
    }
}
