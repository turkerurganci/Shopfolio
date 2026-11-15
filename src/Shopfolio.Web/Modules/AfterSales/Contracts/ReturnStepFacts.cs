namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// A withdrawal whose goods the receipt step may still take in (T528; 07 UC-140, UC-142; K-794): its units on the way back (<paramref name="Awaiting"/>) and those of its open
/// "mal dönmedi" closures (<paramref name="NotReturned"/>), which a receipt reopens. Oldest declaration first.
/// </summary>
public sealed record ReturnableWithdrawal(Guid WithdrawalId, Guid OrderItemId, DateOnly DeclarationDate, DateTimeOffset DeclaredAt, int Awaiting, int NotReturned);

/// <summary>A receipt of returned goods (06 4.26): its units, its arrival date and when its units were added to the stock (null: "Stoğa ekle" not done yet).</summary>
public sealed record ReturnReceiptFact(Guid ReturnId, Guid OrderItemId, int Quantity, DateOnly ArrivalDate, DateTimeOffset? StockAddedAt);

/// <summary>
/// A termination or a defect claim whose taken-back goods the receipt step may still take in (T528; 03 1.11.28, 8.3.2.1; D-656, D-911): the units terminated or claimed as defective (K-808) and not
/// received yet. Terminations first, oldest first, then the item's claim.
/// </summary>
public sealed record ReturnableTakenBack(TakenBackSource Source, Guid OrderItemId, int Awaiting);

/// <summary>
/// The return steps' facts of an order's items (T528; 04 9.9.27, 9.9.28): the withdrawals the receipt and the closure act on, the terminations and defect claims whose goods the
/// receipt also takes in, and the receipts "Stoğa ekle" acts on.
/// </summary>
public sealed record ReturnStepFacts(IReadOnlyList<ReturnableWithdrawal> Withdrawals, IReadOnlyList<ReturnableTakenBack> TakenBack, IReadOnlyList<ReturnReceiptFact> Receipts)
{
    public static ReturnStepFacts Empty { get; } = new([], [], []);
}
