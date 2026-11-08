using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The payment provider's notification endpoint (UC-80; 07 4.30.1; 08 2.1.5; 05 4.5.2, 4.5.5): the endpoint's own part of the signed endpoint
/// pipeline (<see cref="SignedEndpointHandler"/>), which owns the body limit, the 400 of a signature that does not hold, the transaction, the 200
/// "received" after the commit and the 500 of a record that cannot be written. This part verifies the signature in the link layer and records the
/// verified notification: the inbound record and, when the notification is to be processed, the job row that processes it - both in the pipeline's
/// transaction (05 4.3.1). A marked notification (<see cref="PaymentNotificationRule"/>) is recorded with its reason and its processing moment and gets
/// no job row. The endpoint never writes the order's state: the job applies the outcome through the transition unit (05 4.5.3).
/// </summary>
internal sealed partial class PaymentNotificationEndpoint : ISignedEndpoint<PaymentNotice>
{
    /// <summary>The provider's "received" body (08 2.1.5): plain text, nothing before or after it.</summary>
    public const string ReceivedBody = "OK";

    private readonly AppDbContext _db;
    private readonly IJobQueue _jobs;
    private readonly IOrderingModule _ordering;
    private readonly TimeProvider _time;
    private readonly ILogger<PaymentNotificationEndpoint> _logger;
    private readonly bool _isLiveInstallation;
    private readonly IPaymentGateway? _gateway;

    /// <summary>
    /// The provider's gateway is registered by the provider's task with its keys (08 2.1.2; 09 11.1 item 1); without it no signature can be verified, so
    /// every request is refused as one whose signature does not hold - the card method is not offered either (08 2.1 condition 1).
    /// </summary>
    public PaymentNotificationEndpoint(
        AppDbContext db,
        IJobQueue jobs,
        IOrderingModule ordering,
        TimeProvider time,
        IHostEnvironment environment,
        ILogger<PaymentNotificationEndpoint> logger,
        IPaymentGateway? gateway = null)
    {
        ArgumentNullException.ThrowIfNull(environment);
        _db = db;
        _jobs = jobs;
        _ordering = ordering;
        _time = time;
        _logger = logger;
        _isLiveInstallation = environment.IsProduction();
        _gateway = gateway;
    }

    public string Acknowledgement => ReceivedBody;

    public PaymentNotice? Verify(IReadOnlyDictionary<string, string> formFields)
    {
        ArgumentNullException.ThrowIfNull(formFields);
        return _gateway?.VerifyNotification(formFields) is NotificationVerification.SignatureValid valid ? valid.Notice : null;
    }

    public async Task<InboundRecordOutcome> RecordAsync(PaymentNotice notice, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notice);
        IPaymentGateway gateway = _gateway ?? throw new InvalidOperationException("A notice is verified only by the payment gateway.");
        OrderForPayment? order = notice.OrderNumber is { } number ? await _ordering.FindOrderForPaymentAsync(number, ct) : null;
        UnprocessedNotificationReason? reason = PaymentNotificationRule.Classify(notice, order, gateway, _isLiveInstallation);

        Guid id = EntityIds.New();
        DateTimeOffset now = _time.GetUtcNow();

        // The notification itself is unique (06 4.35; K-970): the same one again - by the unique index, also when two copies race - writes nothing.
        // The record carries no personal field: the provider's reference, kept as it came in both reference columns (08 2.1.5), the amount and the moments.
        InboundNotificationRow row = new(
            id,
            now,
            notice.ProviderTransactionId,
            notice.EventType,
            notice.Outcome,
            notice.ProviderTransactionId,
            PaymentNotificationRule.RecordedAmount(notice),
            order?.OrderId,
            reason is null ? null : now,
            reason);
        if (!await InboundNotificationInsert.TryInsertAsync(_db, row, ct))
        {
            return new InboundRecordOutcome.AlreadyRecorded();
        }

        if (reason is { } marked)
        {
            // A marked notification is written to the application log too (05 4.5.5): the reason and the record's id, no reference and no amount.
            LogUnprocessed(_logger, LogEvents.InboundNotificationUnprocessed, marked, id);
            return new InboundRecordOutcome.Recorded();
        }

        await _jobs.EnqueueAsync(JobType.InboundNotificationProcessing, id, notBefore: null, ct);
        return new InboundRecordOutcome.Recorded();
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {Reason} {RecordId}")]
    private static partial void LogUnprocessed(ILogger logger, string @event, UnprocessedNotificationReason reason, Guid recordId);
}
