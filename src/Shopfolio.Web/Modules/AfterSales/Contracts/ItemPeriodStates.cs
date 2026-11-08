using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// An item as the period rules of the after-sales operations read it (03 1.11.5, 1.11.6, 1.11.9, 1.11.28-1.11.30; T323): its type and delivery date and mark, its order's
/// status, confirmation moment and delivery date (the physical items'), whether the order carried a physical item, and the item's frozen withdrawal exemption. The caller -
/// the order module's operation decision - hands the values in; the periods themselves stay in this module (02 4.2 Z-13, Z-14, Z-18, Z-42).
/// </summary>
public sealed record ItemPeriodQuery(
    Guid OrderItemId,
    ProductType ProductType,
    OrderStatus OrderStatus,
    DateOnly? DeliveryDate,
    DateTimeOffset? DeliveryMarkedAt,
    DateTimeOffset OrderCreatedAt,
    bool OrderHasPhysicalItem,
    DateOnly? OrderDeliveryDate,
    WithdrawalExemptionReason? Exemption);

/// <summary>
/// What the item's records and periods say at one moment (T323; K-794): <paramref name="ClosedUnits"/> - the units its cancellation, removal, late delivery termination and
/// withdrawal records closed; <paramref name="WithdrawalWindowOpen"/> and <paramref name="WithdrawalLastDay"/> - Z-13 or Z-14 as 03 4.1.14-4.1.16 read it (the last day is null
/// while the window has no running period); <paramref name="DefectChannelOpen"/> and <paramref name="DefectLastDay"/> - Z-18; <paramref name="DefectClaim"/> - the item's one
/// claim; <paramref name="IsExempt"/> and <paramref name="IsConditionallyExempt"/> - the frozen exemption excludes the withdrawal, or holds only while the packaging is unopened;
/// <paramref name="ReceivableUnits"/> - the withdrawn units whose goods the firm may still take in (the ones on their way back and the ones closed as not come back);
/// <paramref name="RejectableUnits"/> - the units on their way back, on an item with a conditional exemption; <paramref name="NotReturnedClosableUnits"/> - the units on their
/// way back whose sending period (Z-42) has run out.
/// </summary>
public sealed record ItemPeriodState(
    int ClosedUnits,
    bool WithdrawalWindowOpen,
    DateOnly? WithdrawalLastDay,
    bool DefectChannelOpen,
    DateOnly? DefectLastDay,
    DefectClaimStatus? DefectClaim,
    bool IsExempt,
    bool IsConditionallyExempt,
    int ReceivableUnits,
    int RejectableUnits,
    int NotReturnedClosableUnits);

/// <summary>An order's refund IBAN as the operation decision reads it (03 1.11.11, 1.11.12, 1.11.39): whether one is on file and whether a request for one is open on an item.</summary>
public sealed record RefundIbanState(bool HasIban, bool HasOpenRequest);
