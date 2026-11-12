namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// One defect claim as the panel's requests list shows it (T532; 04 9.10.4): the claim, the item it is about, the defective units, the status, <see cref="CreatedAt"/> - the moment the claim was first opened - and <see cref="OpenedAt"/> - the moment the claim was opened,
/// or the moment of its last reopening, which is the list's order key (04 9.10.6; K-809). The item's name and the order number are read from the Ordering surface by the caller.
/// </summary>
public sealed record PanelDefectClaimRow(Guid ClaimId, Guid OrderItemId, int Quantity, DefectClaimStatus Status, DateTimeOffset CreatedAt, DateTimeOffset OpenedAt);
