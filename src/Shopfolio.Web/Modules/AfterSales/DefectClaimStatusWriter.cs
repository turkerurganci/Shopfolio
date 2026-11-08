using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The one caller of <see cref="DefectClaim.ApplyStatus"/> (09 9.1 item 4; D-274): the claim's own transitions of 03 1.9.1. Open to Resolved is the administrator's
/// "resolved" mark; Resolved to Open is the reopening by the customer or an administrator, and only the customer may raise the defective units with it (never lower
/// them). The window (Z-18) and the upper bound of the units are checked by the module's writer before this call; the status and its history entry are written
/// together. The writer is reached only through <see cref="IAfterSalesModule.ChangeDefectClaimStatusAsync"/>, which the transition unit's operations call under the
/// order's lock (09 6 item 2: the claim is the AfterSales module's entity).
/// </summary>
internal static class DefectClaimStatusWriter
{
    public static ItemRecordRefusal? TryChange(DefectClaim claim, DefectClaimStatus status, ActorKind actor, int? quantity, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(claim);
        if (actor is not (ActorKind.Buyer or ActorKind.Admin))
        {
            return ItemRecordRefusal.ActorNotAllowed;
        }

        switch (status)
        {
            case DefectClaimStatus.Resolved:
                if (actor != ActorKind.Admin || quantity is not null)
                {
                    return ItemRecordRefusal.ActorNotAllowed;
                }

                if (claim.DefectClaimStatus != DefectClaimStatus.Open)
                {
                    return ItemRecordRefusal.StatusNotAllowed;
                }

                claim.ApplyStatus(DefectClaimStatus.Resolved, actor, at, null);
                return null;

            case DefectClaimStatus.Open:
                if (claim.DefectClaimStatus != DefectClaimStatus.Resolved)
                {
                    return ItemRecordRefusal.StatusNotAllowed;
                }

                if (quantity is { } raised && raised != claim.Quantity)
                {
                    if (actor != ActorKind.Buyer)
                    {
                        return ItemRecordRefusal.ActorNotAllowed;
                    }

                    if (raised < claim.Quantity)
                    {
                        return ItemRecordRefusal.QuantityExceedsOpen;
                    }

                    claim.ApplyStatus(DefectClaimStatus.Open, actor, at, raised);
                    return null;
                }

                claim.ApplyStatus(DefectClaimStatus.Open, actor, at, null);
                return null;

            default:
                return ItemRecordRefusal.StatusNotAllowed;
        }
    }
}
