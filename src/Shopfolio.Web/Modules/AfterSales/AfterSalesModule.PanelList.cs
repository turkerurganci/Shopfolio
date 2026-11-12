using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// What the panel's requests list reads of the defect claims (T532; 07 UC-93; 04 9.10.4, 9.10.6). A plain read: nothing is written, locked or kept. The list is ordered in the database by the claim's last
/// "Open" entry of the status history, so a reopened claim comes with its last reopening and no page takes the growing set whole (09 20 item 2). The raw statement only adds that moment to the claim's own
/// columns (<c>SELECT d.*</c>); the status filter, the order and the limit are composed over it in the query, so the statement names no status column (09 9.1 item 8).
/// </summary>
internal sealed partial class AfterSalesModule
{
    public async Task<int> CountPanelDefectClaimsAsync(IReadOnlyCollection<DefectClaimStatus> statuses, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(statuses);
        if (statuses.Count == 0)
        {
            return await _db.DefectClaims.AsNoTracking().CountAsync(ct);
        }

        DefectClaimStatus[] named = [.. statuses];
        return await _db.DefectClaims.AsNoTracking().CountAsync(c => named.Contains(c.DefectClaimStatus), ct);
    }

    public async Task<IReadOnlyList<PanelDefectClaimRow>> ListPanelDefectClaimsAsync(IReadOnlyCollection<DefectClaimStatus> statuses, int take, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(statuses);
        ArgumentOutOfRangeException.ThrowIfNegative(take);
        if (take == 0)
        {
            return [];
        }

        // The status history is a jsonb list of {At, Status, ActorKind, Quantity} (PayloadJson: member names as they are, enums as code names); the claim's last opening is the latest "Open" entry's moment,
        // and a claim never reopened has no such entry: its opening is its creation.
        IQueryable<PanelClaimLine> source = _db.Database.SqlQuery<PanelClaimLine>($"""
            SELECT d.*,
                   COALESCE((SELECT max((e.value ->> 'At')::timestamptz) FROM jsonb_array_elements(d.status_history) AS e(value) WHERE e.value ->> 'Status' = 'Open'), d.created_at) AS opened_at
            FROM defect_claims AS d
            """);
        if (statuses.Count > 0)
        {
            string[] named = [.. statuses.Select(s => s.ToString())];
            source = source.Where(l => named.Contains(l.StatusName));
        }

        List<PanelClaimLine> lines = await source.OrderByDescending(l => l.OpenedAt).ThenByDescending(l => l.ClaimId).Take(take).ToListAsync(ct);
        return [.. lines.Select(l => new PanelDefectClaimRow(l.ClaimId, l.OrderItemId, l.Quantity, Enum.Parse<DefectClaimStatus>(l.StatusName), l.CreatedAt, l.OpenedAt))];
    }

    /// <summary>One line of the raw list query: the claim's own columns and the moment of its last opening; the status is the stored code name.</summary>
    internal sealed class PanelClaimLine
    {
        [Column("id")]
        public Guid ClaimId { get; set; }

        [Column("order_item_id")]
        public Guid OrderItemId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("defect_claim_status")]
        public string StatusName { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("opened_at")]
        public DateTimeOffset OpenedAt { get; set; }
    }
}
