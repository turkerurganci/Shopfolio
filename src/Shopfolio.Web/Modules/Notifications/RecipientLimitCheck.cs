using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>What asking the per-recipient limit about one dispatch row came to.</summary>
internal enum RecipientLimitVerdict
{
    /// <summary>The row is not a customer notice with an order link (a firm notice, an account or administration e-mail): not limited, not counted.</summary>
    NotLimited,

    /// <summary>The row is not a "queued" row that was never tried: the limit is asked once, at the first attempt, and not again on a retry.</summary>
    NotFirstAttempt,

    /// <summary>A resending or a B-1 after a correction: counted by later rows, never refused (08 2.2.4 "Muafiyet").</summary>
    Exempt,

    /// <summary>The count of earlier rows is under the threshold: the row goes to the sending service.</summary>
    Allowed,

    /// <summary>The count has reached the threshold: the row was written "undelivered" with the recipient error and the sender makes no call.</summary>
    Refused,
}

/// <summary>
/// The per-recipient sending limit asked at the first attempt of a dispatch row (08 2.2.2 call conditions (3), 2.2.4; 02 9.1.2, 9.1.6; 06 4.58;
/// K-1242). The sender that picks the "queued" rows (T305, T385, T407; D-97) calls <see cref="AskAsync"/> before it makes the body or calls the
/// service, and calls the service only for <see cref="RecipientLimitVerdict.Allowed"/>, <see cref="RecipientLimitVerdict.Exempt"/> and
/// <see cref="RecipientLimitVerdict.NotLimited"/>. A refused row is written in its own transaction and nothing else is: no order, payment or
/// transition, no channel state, no other dispatch row - the order's "e-mail did not arrive" mark is read from the row's state
/// (<see cref="DispatchMarks"/>). The answer depends on the row's place in the order of creation, not on the moment of asking (08 2.2.4), so a
/// queue that built up is judged the same as a row asked at once. The counter is read from the dispatch record's index on the moment
/// (06 7.5); the recipient is compared in memory with the identity normalization because the record keeps the address as entered and
/// no normalized column (06 4.58, 2.8.1).
/// </summary>
internal sealed class RecipientLimitCheck
{
    private readonly AppDbContext _db;
    private readonly ITransactionRunner _transactions;
    private readonly RecipientLimitLog _log;

    public RecipientLimitCheck(AppDbContext db, ITransactionRunner transactions, RecipientLimitLog log)
    {
        _db = db;
        _transactions = transactions;
        _log = log;
    }

    /// <summary>Asks the limit about the row <paramref name="dispatchId"/>; a refused row is written "undelivered" before this returns.</summary>
    public async Task<RecipientLimitVerdict> AskAsync(Guid dispatchId, CancellationToken ct)
    {
        var row = await _db.DispatchRecords.AsNoTracking()
            .Where(d => d.Id == dispatchId)
            .Select(d => new
            {
                d.Id,
                d.CreatedAt,
                d.DispatchType,
                d.OrderId,
                d.RecipientEmail,
                d.OriginalDispatchId,
                d.DispatchState,
                d.AttemptCount,
                d.LastOutcome,
            })
            .SingleOrDefaultAsync(ct);

        if (row is null || row.DispatchState != DispatchState.Queued || row.AttemptCount != 0 || row.LastOutcome is not null)
        {
            return RecipientLimitVerdict.NotFirstAttempt;
        }

        if (!RecipientSendLimit.TakesPart(row.DispatchType, row.OrderId))
        {
            return RecipientLimitVerdict.NotLimited;
        }

        if (RecipientSendLimit.IsExempt(row.OriginalDispatchId))
        {
            return RecipientLimitVerdict.Exempt;
        }

        if (row.RecipientEmail is null)
        {
            return RecipientLimitVerdict.Allowed;
        }

        DateTimeOffset start = RecipientSendLimit.WindowStart(row.CreatedAt);
        List<RecipientSendLimit.Candidate> candidates = await _db.DispatchRecords.AsNoTracking()
            .Where(d => d.OrderId != null && d.RecipientEmail != null && d.CreatedAt >= start && d.CreatedAt <= row.CreatedAt)
            .Select(d => new RecipientSendLimit.Candidate(d.Id, d.CreatedAt, d.RecipientEmail))
            .ToListAsync(ct);

        int earlier = RecipientSendLimit.CountEarlier(row.CreatedAt, row.Id, row.RecipientEmail, candidates);
        if (!RecipientSendLimit.IsOver(earlier))
        {
            return RecipientLimitVerdict.Allowed;
        }

        RefuseResult written = await _transactions.RunAsync(
            async cancel =>
            {
                DispatchRecord? queued = await _db.DispatchRecords.SingleOrDefaultAsync(
                    d => d.Id == dispatchId && d.DispatchState == DispatchState.Queued && d.AttemptCount == 0,
                    cancel);
                if (queued is null)
                {
                    return new RefuseResult(false);
                }

                RecipientSendLimit.Refuse(queued);
                return new RefuseResult(true);
            },
            ct);

        if (!written.Applied)
        {
            return RecipientLimitVerdict.NotFirstAttempt;
        }

        _log.Record();
        return RecipientLimitVerdict.Refused;
    }

    private sealed record RefuseResult(bool Applied) : IOperationResult
    {
        public bool IsRejected => !Applied;
    }
}
