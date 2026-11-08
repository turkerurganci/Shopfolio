using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The one raw statement of the notification endpoint (T311; 06 4.35; 07 4.30.1 Idempotency; K-970): the inbound record written with
/// <c>ON CONFLICT DO NOTHING</c>, so the same notification again - by the record's unique index, also when two copies race - writes nothing and the
/// caller's transaction stays usable (a failed insert would abort it). The statement runs inside the signed endpoint pipeline's open transaction. The
/// inbound record is no row of the order side (09 10.2 items 2-4): it is written by no save and passes no version guard. It lives alone in this file so
/// that the closed list of places of the bulk write scan names this statement and nothing else of the Payment module.
/// </summary>
internal static class InboundNotificationInsert
{
    /// <summary>Writes the record; true when it was written, false when the same notification is already recorded.</summary>
    public static async Task<bool> TryInsertAsync(AppDbContext db, InboundNotificationRow row, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(row);
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The inbound record is written only inside the caller's transaction (09 9.1 item 3).");
        }

        string? reason = row.UnprocessedReason?.ToString();
        int written = await db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO inbound_notification_records
                (id, created_at, provider_transaction_id, event_type, outcome, order_reference, amount, order_id, processed_at, unprocessed_reason)
            VALUES
                ({row.Id}, {row.CreatedAt}, {row.ProviderTransactionId}, {row.EventType}, {row.Outcome}, {row.OrderReference}, {row.Amount},
                 {row.OrderId}, {row.ProcessedAt}, {reason})
            ON CONFLICT DO NOTHING
            """,
            ct);
        return written == 1;
    }
}

/// <summary>The values of one inbound record (06 4.35); no personal field.</summary>
internal sealed record InboundNotificationRow(
    Guid Id,
    DateTimeOffset CreatedAt,
    string ProviderTransactionId,
    string EventType,
    string Outcome,
    string OrderReference,
    decimal? Amount,
    Guid? OrderId,
    DateTimeOffset? ProcessedAt,
    UnprocessedNotificationReason? UnprocessedReason);
