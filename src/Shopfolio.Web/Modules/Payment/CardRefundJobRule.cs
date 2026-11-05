using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
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
/// the result is the payment provider task's (T405) and calls this rule first.
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
        ArgumentNullException.ThrowIfNull(db);
        return await db.RefundRecords
            .AsNoTracking()
            .Where(r => r.RefundId == refundId && r.CreatedAt >= jobCreatedAt)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => (RefundRecordState?)r.State)
            .FirstOrDefaultAsync(ct);
    }
}
