namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The open face of the publish status transitions of a product and of a variant (03 1.8.1; 02 3.7.1, 5.1; 09 9.1 item 6): what the panel calls. The
/// two carry the three values separately and every change between them is free - Draft, Published, Archived, six moves each - and the administrator's
/// own panel operation; it raises no notification and writes one trail row (<c>ChangePublishStatus</c>) in the caller's transaction. The one limit is
/// the publish gate: a move to Published asks it and a refusal leaves the status as it was, and the move that would break the gate of a published
/// product - its last Published variant leaving Published - is refused too (03 1.8.2). An item that leaves the showcase leaves the carts in the same
/// operation (03 1.8.3). The methods join the caller's transaction and need one (09 9.1 item 3).
/// </summary>
public interface IProductTransitions
{
    /// <summary>
    /// Moves a product to <paramref name="target"/>. <paramref name="shownVersion"/> is the version of the product form the screen showed (06 2.6.2).
    /// The product's own status does not depend on its variants' statuses.
    /// </summary>
    Task<PublishChangeResult> ChangeProductStatusAsync(Guid productId, PublishStatus target, int shownVersion, CancellationToken ct);

    /// <summary>Moves a variant to <paramref name="target"/>; the variant's status is its own, whatever its product's status is.</summary>
    Task<PublishChangeResult> ChangeVariantStatusAsync(Guid variantId, PublishStatus target, int shownVersion, CancellationToken ct);
}
