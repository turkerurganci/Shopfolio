using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// What the panel's requests list reads of the defect claims (T532; 07 UC-93; 04 9.10.4, 9.10.6). A plain read: nothing is written, locked or kept. The list is ordered in the database by the claim's last
/// "Open" entry of the status history, so a reopened claim comes with its last reopening and no page takes the growing set whole (09 20 item 2).
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

        // The status history is a jsonb list of {At, Status, ActorKind, Quantity} (PayloadJson: member names as they are, enums as code names); the claim's last opening is the latest "Open" entry's moment.
        string[] named = [.. statuses.Select(s => s.ToString())];
        List<PanelClaimLine> lines = await _db.Database
            .SqlQuery<PanelClaimLine>($"""
                SELECT id AS "ClaimId", order_item_id AS "OrderItemId", quantity AS "Quantity", defect_claim_status AS "Status", created_at AS "CreatedAt",
                       COALESCE((SELECT max((e.value ->> 'At')::timestamptz) FROM jsonb_array_elements(status_history) AS e(value) WHERE e.value ->> 'Status' = 'Open'), created_at) AS "OpenedAt"
                FROM defect_claims
                WHERE cardinality({named}) = 0 OR defect_claim_status = ANY({named})
                ORDER BY "OpenedAt" DESC, id DESC
                LIMIT {take}
                """)
            .ToListAsync(ct);
        return [.. lines.Select(l => new PanelDefectClaimRow(l.ClaimId, l.OrderItemId, l.Quantity, Enum.Parse<DefectClaimStatus>(l.Status), l.CreatedAt, l.OpenedAt))];
    }

    /// <summary>One line of the raw list query: the status is the stored code name.</summary>
    internal sealed class PanelClaimLine
    {
        public Guid ClaimId { get; set; }

        public Guid OrderItemId { get; set; }

        public int Quantity { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset OpenedAt { get; set; }
    }
}
