using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Fakes;

/// <summary>
/// Hand-written stateful fake of <see cref="IPaymentGateway"/> (09 19.1 items 3 and 6, 11.1 item 8). It never leaves the process: each job
/// returns the next queued result of its family, or the job's default when none is queued, and every call is kept in order with its argument.
/// A test queues any class of a family - the classes a timeout, a broken connection or an unlisted answer become included - so a module test
/// runs one case per class. A single failed call is queued as its class (<see cref="FakeFailures.ResultOf{TResult}"/>); a lasting outage is
/// <see cref="Outage"/> (T204 criterion 1).
/// </summary>
internal sealed class FakePaymentGateway : IPaymentGateway
{
    private readonly object _gate = new();
    private readonly List<FakeCall> _calls = [];
    private readonly Queue<PaymentStartResult> _starts = new();
    private readonly Queue<PaymentQueryResult> _queries = new();
    private readonly Queue<RefundRequestResult> _refunds = new();
    private readonly Queue<RefundQueryResult> _refundQueries = new();
    private readonly Queue<NotificationVerification> _verifications = new();

    public PaymentStartResult DefaultStart { get; set; } = new PaymentStartResult.Started(new Uri("https://payment.invalid/frame"));

    public PaymentQueryResult DefaultQuery { get; set; } = new PaymentQueryResult.NoPayment();

    public RefundRequestResult DefaultRefund { get; set; } = new RefundRequestResult.Accepted();

    public RefundQueryResult DefaultRefundQuery { get; set; } = new RefundQueryResult.NoResult();

    /// <summary>The fake's event type of a payment notification.</summary>
    public const string PaymentEventType = "payment";

    /// <summary>The fake's outcome of a successful payment.</summary>
    public const string SucceededOutcome = "succeeded";

    /// <summary>The fake's outcome of a failed payment.</summary>
    public const string FailedOutcome = "failed";

    public NotificationVerification DefaultVerification { get; set; } = new NotificationVerification.SignatureInvalid();

    /// <summary>
    /// The provider is down while set: every call answers its family's class of this failure, the queues are left as they are and the call is
    /// still kept. The notification's verification is a local computation, not a call (08 3.1 table), and is not affected.
    /// </summary>
    public ExternalCallFailureKind? Outage { get; set; }

    /// <summary>The calls in the order they were made.</summary>
    public IReadOnlyList<FakeCall> Calls
    {
        get
        {
            lock (_gate)
            {
                return [.. _calls];
            }
        }
    }

    public void EnqueueStart(params PaymentStartResult[] results) => Enqueue(_starts, results);

    public void EnqueueQuery(params PaymentQueryResult[] results) => Enqueue(_queries, results);

    public void EnqueueRefund(params RefundRequestResult[] results) => Enqueue(_refunds, results);

    public void EnqueueRefundQuery(params RefundQueryResult[] results) => Enqueue(_refundQueries, results);

    public void EnqueueVerification(params NotificationVerification[] results) => Enqueue(_verifications, results);

    public Task<PaymentStartResult> StartAsync(PaymentStartRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(NextCall(nameof(StartAsync), request, _starts, DefaultStart));
    }

    public Task<PaymentQueryResult> QueryPaymentAsync(string orderNumber, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(NextCall(nameof(QueryPaymentAsync), orderNumber, _queries, DefaultQuery));
    }

    public Task<RefundRequestResult> RequestRefundAsync(CardRefund refund, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(NextCall(nameof(RequestRefundAsync), refund, _refunds, DefaultRefund));
    }

    public Task<RefundQueryResult> QueryRefundAsync(CardRefund refund, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(NextCall(nameof(QueryRefundAsync), refund, _refundQueries, DefaultRefundQuery));
    }

    public NotificationVerification VerifyNotification(IReadOnlyDictionary<string, string> formFields)
    {
        return Next(nameof(VerifyNotification), formFields, _verifications, DefaultVerification);
    }

    /// <summary>
    /// The fake's own values, no provider's: event type <see cref="PaymentEventType"/> with outcome <see cref="SucceededOutcome"/> or
    /// <see cref="FailedOutcome"/>; anything else is unrecognized. A local computation, not a call: not kept, not affected by an outage.
    /// </summary>
    public NotificationResult ReadNotificationResult(string eventType, string outcome)
    {
        if (!string.Equals(eventType, PaymentEventType, StringComparison.Ordinal))
        {
            return NotificationResult.Unrecognized;
        }

        return outcome switch
        {
            SucceededOutcome => NotificationResult.PaymentSucceeded,
            FailedOutcome => NotificationResult.PaymentFailed,
            _ => NotificationResult.Unrecognized,
        };
    }

    /// <summary>
    /// The fake's amount rule, the shape of 08 2.1.5's on the product's values: the started amount equals the order's total and the collected total is
    /// not below it. A local computation, not a call: not kept.
    /// </summary>
    public NotificationAmountCheck CheckNotificationAmount(PaymentNotice notice, decimal orderTotal)
    {
        ArgumentNullException.ThrowIfNull(notice);
        return notice.PaymentAmount == orderTotal && notice.TotalAmount >= orderTotal ? NotificationAmountCheck.Holds : NotificationAmountCheck.Mismatch;
    }

    // A call to the provider: during an outage it answers the failure's class; otherwise the queue, then the default.
    private T NextCall<T>(string job, object argument, Queue<T> queue, T fallback)
        where T : IExternalCallResult<T>
    {
        lock (_gate)
        {
            if (Outage is { } kind)
            {
                _calls.Add(new FakeCall(job, argument));
                return FakeFailures.ResultOf<T>(kind);
            }

            return Next(job, argument, queue, fallback);
        }
    }

    private void Enqueue<T>(Queue<T> queue, T[] results)
    {
        lock (_gate)
        {
            foreach (T result in results)
            {
                queue.Enqueue(result);
            }
        }
    }

    private T Next<T>(string job, object argument, Queue<T> queue, T fallback)
    {
        lock (_gate)
        {
            _calls.Add(new FakeCall(job, argument));
            return queue.Count > 0 ? queue.Dequeue() : fallback;
        }
    }
}

/// <summary>One call to a fake gateway: the interface member and its argument.</summary>
internal sealed record FakeCall(string Job, object Argument);
