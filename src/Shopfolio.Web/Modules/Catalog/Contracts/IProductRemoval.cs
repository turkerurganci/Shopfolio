namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// What a permanent deletion touches, for the panel's confirmation (02 3.7.7, 3.7.8): the open orders - payment awaited, or paid and not completed - that hold the record, the
/// units of the record's reservations that fall with it, and whether the variant is the last Published one of a published product (the deletion is then refused). A product
/// has none of the last. The panel says the open order count before the confirmation; an open order does not stop the deletion.
/// </summary>
public sealed record RemovalImpact(int OpenOrderCount, int ReservedQuantity, bool IsLastPublishedVariant);

/// <summary>
/// The permanent deletion of a product and of a variant, and the removal of a digital file - the blocking pattern of the published record (02 3.7.4, 3.7.7, 3.7.8; 03 1.8.2;
/// 09 9.1 item 6): what the panel calls. A deletion is permanent and is not the archive: the record, its variants, options, category and image links and price history go with the
/// product; the order items keep their frozen content and only lose the link to the deleted record; an open order is no obstacle, and the reserved units of a deleted variant
/// fall with it; the carts lose the items. The one thing that stops a deletion is the published product's gate: the last Published variant of a published product is not deleted
/// (<c>LastPublishedVariant</c>), and the panel sends the administrator to take the product to draft or to the archive first. A file's removal that would leave a Published
/// variant of a published digital product without a file is refused (<c>DigitalFileRequired</c>). The methods join the caller's transaction and need one (09 9.1 item 3); they
/// raise no notification and write no trail row (the trail's areas are closed, 02 10.3.1).
/// </summary>
public interface IProductRemoval
{
    /// <summary>The open order count and the reservation a product's deletion would take; null when the product is gone. A plain read.</summary>
    Task<RemovalImpact?> GetProductImpactAsync(Guid productId, CancellationToken ct);

    /// <summary>The open order count and the reservation a variant's deletion would take, and whether it is refused as the last Published variant; null when the variant is gone. A plain read.</summary>
    Task<RemovalImpact?> GetVariantImpactAsync(Guid variantId, CancellationToken ct);

    /// <summary>Deletes a product for good. <paramref name="shownVersion"/> is the version of the product form the screen showed (06 2.6.2).</summary>
    Task<RemovalResult> DeleteProductAsync(Guid productId, int shownVersion, CancellationToken ct);

    /// <summary>Deletes a variant for good; <paramref name="shownVersion"/> is the product form's version (the variant is a member of the form, 06 2.6.2).</summary>
    Task<RemovalResult> DeleteVariantAsync(Guid variantId, int shownVersion, CancellationToken ct);

    /// <summary>
    /// Removes a digital file: the row is detached and stays - an order item may still point to it (06 4.70). Refused when it leaves a Published variant of a published digital
    /// product without a file. The file's row lock is taken first (09 9.1 item 5).
    /// </summary>
    Task<RemovalResult> RemoveDigitalFileAsync(Guid fileId, CancellationToken ct);
}
