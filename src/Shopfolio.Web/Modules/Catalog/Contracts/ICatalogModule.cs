namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The open surface of the Catalog module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns products, categories, discounts, coupons and files (05 3.3.2).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface ICatalogModule
{
    /// <summary>
    /// Reserves stock, service capacity and the coupon right (05 7.3.3, 4.2.2; 06 4.14 path 1): each reservation is written only if what is left
    /// is enough - one conditional update per row, nothing read before. <paramref name="releasedFirst"/> is the earlier unpaid order's
    /// reservation that the same confirmation frees (06 4.14 path 3); its rows and the new ones are written together in the lock order - the
    /// variants by id, the freeing of a variant before its new reservation, the coupon last (05 7.3.5). False when one reservation did not fit;
    /// the caller refuses and the transaction undoes the rows already written. The product's version does not change (K-980).
    /// </summary>
    Task<bool> ReserveAsync(StockClaim claim, StockClaim? releasedFirst, CancellationToken ct);

    /// <summary>Frees a reservation (06 4.14 path 3): the reserved quantity and the reserved coupon right go down; in the lock order.</summary>
    Task ReleaseAsync(StockClaim claim, CancellationToken ct);

    /// <summary>
    /// The final decrease at payment (06 4.14 path 2): the stock - when it is not unlimited - and the reserved quantity go down by the
    /// reservation, the reserved coupon right becomes a used one; in the lock order.
    /// </summary>
    Task CommitAsync(StockClaim claim, CancellationToken ct);

    /// <summary>
    /// The return of a paid order's units and right (06 4.14, the paragraph after path 6): the stock or capacity goes up when it is not unlimited
    /// and the variant still exists, the used coupon right is given back; in the lock order.
    /// </summary>
    Task ReturnAsync(StockClaim claim, CancellationToken ct);

    /// <summary>
    /// The panel's write of the stock or capacity field (05 7.3.1; 06 2.6.5; 02 §3.6.7): one conditional update - the new value is written only if
    /// the field still holds the value the form showed and the new value is not below the reserved quantity. The product's version does not
    /// change; the field is outside the product form's version (K-980).
    /// </summary>
    Task<StockQuantityWriteResult> SaveStockQuantityAsync(Guid variantId, int? shownValue, int? newValue, CancellationToken ct);
}
