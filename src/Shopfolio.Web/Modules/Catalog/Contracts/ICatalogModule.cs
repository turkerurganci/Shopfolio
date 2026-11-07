using Shopfolio.Modules.Catalog.Contracts.Variants;

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

    /// <summary>
    /// An image upload (09 15.6 items 5, 7; 05 7.7.2): the size ceiling (P-23), the type from the content, the pixel ceiling, the re-encoded source
    /// copy in the file store and - in the caller's open transaction, which the method joins and never opens (09 9.1 item 3) - the unlinked image
    /// row and its derivative job. Called outside a transaction it throws at the job row. The refusal is a result, not an exception.
    /// </summary>
    Task<ImageUploadResult> UploadImageAsync(byte[] content, CancellationToken ct);

    /// <summary>
    /// The page addresses the site map takes from this module (05 3.7.3; 02 3.30.5, 3.30.6): <c>/urun/{slug}</c> of every published product and
    /// <c>/kategori/{slug}</c> of every category. An archived product and a draft are not listed. A plain read; the site map's cache is Settings'.
    /// </summary>
    Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct);

    /// <summary>
    /// The file an order item downloads now (05 7.7.3; 02 3.12.7; 06 4.70): the variant's current file when the variant stands - its own, else its product's - and
    /// otherwise the item's last known file; null when there is none or its upload did not complete. A plain read; it writes nothing.
    /// </summary>
    Task<DigitalFileLocation?> FindDownloadFileAsync(Guid? variantId, Guid? lastKnownFileId, CancellationToken ct);

    /// <summary>
    /// Starts a chunked upload (07 4.31.2; 09 15.7 item 5): checks the name and the declared size (400), P-26 and the disk threshold (422) and writes a new
    /// unlinked, incomplete file row. Joins the caller's transaction (09 9.1 item 3).
    /// </summary>
    Task<UploadStartResult> StartUploadAsync(string? fileName, long sizeBytes, CancellationToken ct);

    /// <summary>
    /// Takes one chunk of a chunked upload (07 4.31.2; 09 15.7 item 6) under the file row's lock: the received bytes are read from the file store inside the
    /// lock, a chunk that starts elsewhere is refused, the bytes are appended and the last one completes the upload in the same transaction. A request that
    /// finds every byte written but the upload not completed - a crash after the last chunk - completes it without appending (K-1203). Joins the caller's
    /// transaction and must run inside one.
    /// </summary>
    Task<UploadChunkResult> ReceiveChunkAsync(Guid id, long start, Stream body, long length, CancellationToken ct);

    /// <summary>The state of a chunked upload (07 4.31.2): null when the upload is unknown or was swept. A plain read; it writes nothing.</summary>
    Task<UploadStatus?> GetUploadStatusAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// The storefront's product list and search (02 5.1; 09 10.5): only Published products, newest first, 24 a page. A <paramref name="search"/> that is not null filters by
    /// the name; a word that makes no pattern gives an empty page. A Draft and an Archived product are in neither; a sold-out Published product is in both, marked
    /// (02 3.6.4). A plain read; it writes nothing.
    /// </summary>
    Task<ShowcasePage> ListShowcaseAsync(string? search, int page, CancellationToken ct);

    /// <summary>
    /// The option list of a product page (02 5.1; 03 1.8.1): the product's Published variants when the product is Published, otherwise none. A Draft and an Archived variant
    /// are not listed; a sold-out one is, marked (02 3.6.3). A plain read; it writes nothing.
    /// </summary>
    Task<IReadOnlyList<ShowcaseVariant>> ListOptionVariantsAsync(Guid productId, CancellationToken ct);

    /// <summary>
    /// The stock code the new variant's form is filled with (02 3.3.4; 05 7.8.1, MK-32): the product's address in capitals - its first twenty characters - a hyphen and
    /// the two-digit order of the variant, raised until no other variant holds the code. A saved product (<paramref name="productId"/>) starts from its address and
    /// the order of its next variant; a product not saved yet starts from <paramref name="productName"/> as it is now; null when there is neither a saved product nor a
    /// name - the field then stays empty and required. Only the new variant's form asks for it: a code already given never changes with the name. A plain read.
    /// </summary>
    Task<string?> SuggestStockCodeAsync(Guid? productId, string? productName, CancellationToken ct);

    /// <summary>
    /// Whether a stock code typed in the variant form can be saved (02 3.3.4; 06 2.8.2): missing when blank, taken when another variant holds it - compared without
    /// letter case, "I", "İ", "ı" and "i" being one letter -, free otherwise. <paramref name="exceptVariantId"/> is the variant being edited. The code itself is kept as
    /// typed. A plain read; the unique index is the last defense of two saves that race for one code.
    /// </summary>
    Task<StockCodeCheck> CheckStockCodeAsync(string? stockCode, Guid? exceptVariantId, CancellationToken ct);

    /// <summary>
    /// The verdict on the option values a variant of the product would be bound to (02 3.3.2, 3.3.3; 06 4.6 "Uygulamada"): the values are the product's own - the
    /// first of its option of order 1, the second of its option of order 2 -, every option of the product has its value, and no other variant
    /// (<paramref name="exceptVariantId"/> is the one being edited) holds the combination. A plain read.
    /// </summary>
    Task<VariantStructureCheck> CheckVariantStructureAsync(
        Guid productId,
        Guid? firstOptionValueId,
        Guid? secondOptionValueId,
        Guid? exceptVariantId,
        CancellationToken ct);

    /// <summary>
    /// The option matrix of a product (02 3.3.3): a cell per combination of its option values, each with the variant that opens it or none - a combination the firm
    /// did not open is shown and cannot be chosen. A product without options has one cell with both values empty. Every variant of the product opens its cell,
    /// whatever its status; the publication rules are the caller's. A plain read.
    /// </summary>
    Task<IReadOnlyList<OptionMatrixCell>> GetOptionMatrixAsync(Guid productId, CancellationToken ct);
}
