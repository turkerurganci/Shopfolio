using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>What writing one attempt did: the row took the result, or it was not "queued" any more and nothing was written.</summary>
internal sealed record DispatchAttemptResult(bool Applied, DispatchOutcome Outcome) : IOperationResult
{
    /// <summary>A row that is not "queued" takes no result; the empty transaction is rolled back.</summary>
    public bool IsRejected => !Applied;
}

/// <summary>
/// One attempt of a dispatch record's row (08 2.2.2 "(sonuç)", 2.2 "Retry stratejisi", 3.3; 05 4.4.3, 10.2.2, 10.2.3; 06 4.58, 4.60): the message
/// goes to the e-mail gateway once, outside any transaction (09 10.7 item 5), and the result is then written in one transaction - the row by
/// <see cref="DispatchRetrySchedule"/> and the channel state in the same transaction: a channel error raises the run's count by one (the first
/// error's moment is written when the count goes from zero to one), a sending resets the count, empties the first error's moment and writes the
/// last success, a recipient error neither counts nor resets. The count grows from the row's own value in one update (06 4.60; K-1072). An
/// outage changes nothing else: no other row, no order and no new dispatch row is written - the product sends no notice about the outage
/// (08 3.3) and nothing waits for the e-mail. The sender that picks the "queued" rows, makes the body and calls this is the e-mail tasks'
/// (T305, T385, T407; D-97); a body that cannot be made is recorded as a recipient error without a call (08 2.2 call conditions (5)).
/// </summary>
internal sealed class DispatchAttempts
{
    private readonly AppDbContext _db;
    private readonly ITransactionRunner _transactions;
    private readonly IEmailGateway _gateway;
    private readonly TimeProvider _time;

    public DispatchAttempts(AppDbContext db, ITransactionRunner transactions, IEmailGateway gateway, TimeProvider time)
    {
        _db = db;
        _transactions = transactions;
        _gateway = gateway;
        _time = time;
    }

    /// <summary>Sends <paramref name="message"/> for the row <paramref name="dispatchId"/> and writes the result.</summary>
    public async Task<DispatchAttemptResult> SendAsync(Guid dispatchId, EmailMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);
        DateTimeOffset startedAt = _time.GetUtcNow();
        EmailSendResult result = await _gateway.SendAsync(message, ct);
        return await RecordAsync(dispatchId, result, startedAt, ct);
    }

    /// <summary>
    /// Writes the result of an attempt that started at <paramref name="attemptStartedAt"/> on the row and on the channel state, in one
    /// transaction. A row that is not "queued" (already sent, undelivered, or gone) takes nothing and the channel state is left as it is, so
    /// a result written twice is written once.
    /// </summary>
    public Task<DispatchAttemptResult> RecordAsync(Guid dispatchId, EmailSendResult result, DateTimeOffset attemptStartedAt, CancellationToken ct)
    {
        DispatchOutcome outcome = DispatchRetrySchedule.OutcomeOf(result);
        return _transactions.RunAsync(
            async cancel =>
            {
                DispatchRecord? row = await _db.DispatchRecords.SingleOrDefaultAsync(d => d.Id == dispatchId && d.DispatchState == DispatchState.Queued, cancel);
                if (row is null)
                {
                    return new DispatchAttemptResult(false, outcome);
                }

                DateTimeOffset now = _time.GetUtcNow();
                DispatchRetrySchedule.Apply(row, outcome, attemptStartedAt, now);
                await WriteChannelStateAsync(outcome, now, cancel);
                return new DispatchAttemptResult(true, outcome);
            },
            ct);
    }

    private Task<int> WriteChannelStateAsync(DispatchOutcome outcome, DateTimeOffset now, CancellationToken ct)
    {
        DateTimeOffset? at = now;
        return outcome switch
        {
            DispatchOutcome.ChannelError => _db.ChannelStateRecords.ExecuteUpdateAsync(
                set => set
                    .SetProperty(c => c.FirstChannelErrorAt, c => c.ConsecutiveChannelErrorCount == 0 ? at : c.FirstChannelErrorAt)
                    .SetProperty(c => c.ConsecutiveChannelErrorCount, c => c.ConsecutiveChannelErrorCount + 1),
                ct),
            DispatchOutcome.Sent => _db.ChannelStateRecords.ExecuteUpdateAsync(
                set => set
                    .SetProperty(c => c.ConsecutiveChannelErrorCount, 0)
                    .SetProperty(c => c.FirstChannelErrorAt, (DateTimeOffset?)null)
                    .SetProperty(c => c.LastSucceededAt, at),
                ct),
            DispatchOutcome.RecipientError => Task.FromResult(0),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown dispatch outcome."),
        };
    }
}
