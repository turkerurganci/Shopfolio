using System.Text.RegularExpressions;

using Amazon.Runtime;
using Amazon.SimpleEmailV2.Model;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The error mapping of the send call (08 2.2.2 "Hata eşlemesi"; 09 11.1 item 3, 11.2 item 4): a pure function of the answer of the SendEmail call alone -
/// the answer is a success (<c>null</c>) or the library's exception - and of nothing else. 200 is <see cref="EmailSendResult.Sent"/>; every other
/// answer is a <see cref="EmailSendResult.ChannelError"/>, the ones the table lists (the unverified identity, the rejected content, the invalid request, the
/// account and sending states, the throttling and quota answers, the unknown resource, the refused credential, the 5xx, the unreachable service, the timeout)
/// and the ones it does not. The 404 of the pre-query that asks the recipient is never given to this function (08 2.2.2).
/// The reason code is the service's own error code (or the exception type's name) and never the answer's message, an address or a key.
/// </summary>
internal static partial class SesErrorMapping
{
    /// <summary>The reason code of a rejection that says the sender identity is not verified (08 2.2.2: told apart by the answer's message, checked first).</summary>
    public const string IdentityNotVerifiedReason = "MessageRejected:IdentityNotVerified";

    /// <summary>The reason code of an answer with no error code and no exception type of its own: the status number.</summary>
    public const string StatusReasonPrefix = "http:";

    /// <summary>The reason code of an exception that is none of the library's.</summary>
    public const string UnexpectedReasonPrefix = "unexpected:";

    /// <summary>The reason code of a failure of the library on the client's side (no answer from the service).</summary>
    public const string ClientReason = "client-error";

    public static EmailSendResult Classify(Exception? answer) =>
        answer is null ? new EmailSendResult.Sent() : EmailSendResult.FromFailure(FailureOf(answer));

    /// <summary>The failure of a call without an accepted answer: its kind (for the log) and its reason code; none of them carries a message or a value.</summary>
    public static ExternalCallFailure FailureOf(Exception answer)
    {
        ArgumentNullException.ThrowIfNull(answer);
        return answer switch
        {
            OperationCanceledException or TimeoutException => ExternalCallFailure.TimedOut(),
            HttpRequestException http => ExternalCallFailure.Unreachable(ExternalCallRunner.ConnectionReason(http.HttpRequestError)),
            MessageRejectedException rejected when IdentityNotVerified(rejected.Message) => ExternalCallFailure.Unrecognized(IdentityNotVerifiedReason),
            AmazonServiceException service => ExternalCallFailure.Unrecognized(ServiceReason(service)),
            AmazonClientException => ExternalCallFailure.Unreachable(ClientReason),
            _ => ExternalCallFailure.Unrecognized(UnexpectedReasonPrefix + answer.GetType().Name),
        };
    }

    private static bool IdentityNotVerified(string? message) =>
        message is not null && message.Contains("not verified", StringComparison.OrdinalIgnoreCase);

    private static string ServiceReason(AmazonServiceException exception)
    {
        if (!string.IsNullOrEmpty(exception.ErrorCode) && SafeCode().IsMatch(exception.ErrorCode))
        {
            return exception.ErrorCode;
        }

        string typeName = exception.GetType().Name;
        if (exception.GetType() != typeof(AmazonServiceException) && typeName.EndsWith("Exception", StringComparison.Ordinal))
        {
            return typeName[..^"Exception".Length];
        }

        return StatusReasonPrefix + (int)exception.StatusCode;
    }

    // A service's error code is a short word; anything else (a code that carries a value) falls back to the type's name or the status.
    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9._-]{0,63}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SafeCode();
}
