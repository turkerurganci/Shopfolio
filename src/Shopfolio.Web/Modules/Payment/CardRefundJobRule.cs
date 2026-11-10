using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>What the card refund's handler does with the row it is given (05 4.3.3, 4.6.1).</summary>
internal enum CardRefundStep
{
    /// <summary>No request of this row went out: write the refund record in the "sending" state, let it commit, then send.</summary>
    SendRequest,

    /// <summary>A request went out or may have gone: do not send again; ask for the result like a request of unknown outcome.</summary>
    AskOutcome,

    /// <summary>The result is already written, a "did not happen" result included: send nothing and complete the row.</summary>
    Complete,
}

/// <summary>
/// The rule that keeps a sent card refund from being sent twice when its job row is seen again (05 4.3.3; 4.3.4; MK-40; K-1111; 06 4.34).
/// The runner delivers a row at least once and retries a failed handler by the default rule; the handler of the card refund therefore decides
/// from the refund record, which is written before the request goes. The record of this row is one written at or after the row was created
/// (the firm's retry writes a new row, and the older record of the earlier row is not this row's, 06 4.34). The handler that sends and applies
/// the result, <see cref="CardRefundJobHandler"/> (T405), calls this rule first.
/// </summary>
internal static class CardRefundJobRule
{
    /// <summary>The step for the state of this row's latest refund record; <c>null</c> when this row has written none.</summary>
    public static CardRefundStep Decide(RefundRecordState? latestRecordOfTheRow)
    {
        return latestRecordOfTheRow switch
        {
            null => CardRefundStep.SendRequest,
            RefundRecordState.Sending or RefundRecordState.OutcomePending => CardRefundStep.AskOutcome,
            RefundRecordState.Succeeded or RefundRecordState.Failed => CardRefundStep.Complete,
            _ => throw new ArgumentOutOfRangeException(nameof(latestRecordOfTheRow), latestRecordOfTheRow, "Unknown refund record state."),
        };
    }

    /// <summary>The state of the latest refund record of <paramref name="refundId"/> written at or after <paramref name="jobCreatedAt"/>, or <c>null</c>.</summary>
    public static async Task<RefundRecordState?> ReadLatestStateAsync(AppDbContext db, Guid refundId, DateTimeOffset jobCreatedAt, CancellationToken ct)
    {
        return (await ReadLatestRecordAsync(db, refundId, jobCreatedAt, ct))?.State;
    }

    /// <summary>The latest refund record of <paramref name="refundId"/> written at or after <paramref name="jobCreatedAt"/> - its id and state -, or <c>null</c>.</summary>
    public static async Task<RowRecord?> ReadLatestRecordAsync(AppDbContext db, Guid refundId, DateTimeOffset jobCreatedAt, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        return await db.RefundRecords
            .AsNoTracking()
            .Where(r => r.RefundId == refundId && r.CreatedAt >= jobCreatedAt)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Select(r => new RowRecord(r.Id, r.State))
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// The refund record's outcome of the provider's answer (08 2.1.9; 05 4.6.2): accepted - succeeded, the money went; refused or not sent - failed; unknown - the
    /// outcome is pending and is asked, the request is never sent again.
    /// </summary>
    public static RefundRecordState OutcomeOf(RefundRequestResult answer)
    {
        ArgumentNullException.ThrowIfNull(answer);
        return answer switch
        {
            RefundRequestResult.Accepted => RefundRecordState.Succeeded,
            RefundRequestResult.NotCompleted => RefundRecordState.Failed,
            RefundRequestResult.Unknown => RefundRecordState.OutcomePending,
            _ => throw new ArgumentOutOfRangeException(nameof(answer), answer, "Unknown refund request result."),
        };
    }

    /// <summary>A code error of a refund job (no gateway in the host): no record is written, the runner retries the row by the default rule (05 4.3.4).</summary>
    public static InvalidOperationException CodeError(string reasonCode) => new("The card refund job cannot run: " + reasonCode);

    /// <summary>A refund record as the job reads it: its id and its state.</summary>
    internal sealed record RowRecord(Guid Id, RefundRecordState State);
}
