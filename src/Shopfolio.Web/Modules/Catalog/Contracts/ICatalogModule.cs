using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
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
    /// The wrong cancellation's correction on a paid order (06 4.14 path 6; 03 1.4.4.7): the units the cancellation freed are taken again - each only
    /// if what is left is enough - and fall at once; the coupon right counts as used again, even past its limit. A deleted variant is skipped: it has
    /// no stock to take. False when one variant did not fit; the caller refuses the correction and the transaction undoes the rows already written.
    /// </summary>
    Task<bool> RetakeAsync(StockClaim claim, CancellationToken ct);

    /// <summary>
    /// The transfer mark's correction (06 4.14 path 5; 03 1.4.4.8): the finally decreased units go back to the reservation - the stock, when not
    /// unlimited, and the reserved quantity rise together - and the used coupon right becomes a reserved one. A deleted variant is skipped.
    /// </summary>
    Task RestoreAsync(StockClaim claim, CancellationToken ct);

    /// <summary>
    /// The panel's write of the stock or capacity field (05 7.3.1; 06 2.6.5; 02 §3.6.7): one conditional update - the new value is written only if
    /// the field still holds the value the form showed and the new value is not below the reserved quantity. The product's version does not
    /// change; the field is outside the product form's version (K-980).
    /// </summary>
    Task<StockQuantityWriteResult> SaveStockQuantityAsync(Guid variantId, int? shownValue, int? newValue, CancellationToken ct);

    /// <summary>
    /// The stock field and the reserved quantity beside it (02 3.6.7), for the panel's stock field; null when the variant is gone. The orders that hold the reservation are
    /// the Ordering module's (<c>IOrderingModule.ListReservationHoldersAsync</c>). A plain read; never for a visitor-facing page.
    /// </summary>
    Task<VariantStockState?> GetStockStateAsync(Guid variantId, CancellationToken ct);

    /// <summary>
    /// The sold-out state of a product for the visitor (02 3.6.3-3.6.5): its published variants, each sold out or not, and the product sold out when all are - it then stays
    /// on the shop window, marked, and cannot be added to the cart. Carries no stock number, no threshold and no "notify me" state. A plain read.
    /// </summary>
    Task<ProductAvailability> GetAvailabilityAsync(Guid productId, CancellationToken ct);

    /// <summary>
    /// An image upload (09 15.6 items 5, 7; 05 7.7.2): the size ceiling (P-23), the type from the content, the pixel ceiling, the re-encoded source
    /// copy in the file store and - in the caller's open transaction, which the method joins and never opens (09 9.1 item 3) - the unlinked image
    /// row and its derivative job. Called outside a transaction it throws at the job row. The refusal is a result, not an exception.
    /// </summary>
    Task<ImageUploadResult> UploadImageAsync(byte[] content, CancellationToken ct);

    /// <summary>
    /// Whether an image uploaded through <see cref="UploadImageAsync"/> exists (T362; 06 4.54, 4.69): the module that links an image - the brand's logo and site icon - asks before it writes the
    /// link, so an id that names no image is a refusal and not a foreign key error. A plain read; it writes nothing.
    /// </summary>
    Task<bool> ImageExistsAsync(Guid imageId, CancellationToken ct);

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
    /// What the address <c>/urun/{slug}</c> answers (02 3.7.5, 3.7.6, 3.30.4): a Published product is its page; an Archived product is the "Bu ürün artık satılmıyor" page with its name,
    /// image and status - and no variant, so no price and no cart; a Draft product is "page not found" for a visitor and the page with the "Draft" band for a signed-in administrator
    /// (<paramref name="isAdministrator"/>), who sees the Draft variants too; an address that holds no product, a deleted one among them, is "page not found". A plain read.
    /// </summary>
    Task<ProductPage> GetProductPageAsync(string? slug, bool isAdministrator, CancellationToken ct);

    /// <summary>
    /// The Published variants of a digital product that no file serves (02 3.7.4): the variants the publish gate counts and the panel shows as missing a file - none for a product of
    /// another type, one that has a file at product level, or one whose Published variants each carry their own. A plain read.
    /// </summary>
    Task<IReadOnlyList<Guid>> ListVariantsWithoutFileAsync(Guid productId, CancellationToken ct);

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

    /// <summary>
    /// The price label of a variant for the product page (02 3.8.5, 12.1.5): the price as typed, VAT included; the product's VAT rate; the place of origin when the
    /// page shows its line (physical, filled digital; never a service); the date the price applies since, from the price history; the measurement unit, the net
    /// quantity and the derived unit price of a product sold by measure. Null when there is no such variant. A plain read.
    /// </summary>
    Task<PriceLabel?> GetPriceLabelAsync(Guid variantId, CancellationToken ct);

    /// <summary>
    /// The shipping period of each of the given variants (02 3.20.5): the variant's product, the product's type and the product's own period in business days - null when it sets none
    /// and the firm default applies. A variant takes its product's period; one that does not exist is left out. A plain read.
    /// </summary>
    Task<IReadOnlyList<VariantShippingPeriod>> GetShippingPeriodsAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken ct);

    /// <summary>
    /// Saves a product's dated percentage discount (02 3.9.1, 3.9.2, 3.9.5; 06 4.12): the rate applies to every variant on its own price; the range starts at 00.00 of
    /// the start day and holds the end day whole. The new definition replaces the product's discount. Refused, writing nothing, when the rate is not above zero and below
    /// one hundred with at most two decimals, when the end is before the start, or when a variant's rounded discounted price would be 0,00 TL or would not be below the
    /// reference price the system computes from that variant's price history (02 3.9.3, 3.9.4) - the refusal names the variant. The price history gets the rows the
    /// change owes (06 4.11). Joins the caller's transaction and never saves it (09 9.1 item 3); outside one it throws. The product form raises the product's version.
    /// </summary>
    Task<DiscountSaveResult> SaveDiscountAsync(Guid productId, decimal percentage, DateOnly startDate, DateOnly endDate, CancellationToken ct);

    /// <summary>
    /// Removes a product's discount (06 4.12); removing a running discount writes the variants' own prices to the history (K-1041 (3)). Joins the caller's transaction
    /// and never saves it; outside one it throws.
    /// </summary>
    Task<DiscountRemoveResult> RemoveDiscountAsync(Guid productId, CancellationToken ct);

    /// <summary>
    /// A product's discount as the product form shows it (04 9.5.8): the rate, the dates, the state now, and per variant its price, the reference price the system
    /// computed - read only, never entered (02 3.9.3) - and the discounted price. Null when the product has no discount. A plain read.
    /// </summary>
    Task<DiscountView?> GetDiscountAsync(Guid productId, CancellationToken ct);

    /// <summary>
    /// Changes a variant's price (02 3.8.4, 3.9.1, 3.9.3): refused when the price is not a valid amount, and - while the product's discount has not ended - when its
    /// rounded discounted price would be 0,00 TL; the same price writes nothing. The price history gets the new applied price (06 4.11). Joins the caller's transaction
    /// and never saves it; outside one it throws. The product form raises the product's version.
    /// </summary>
    Task<VariantPriceResult> ChangeVariantPriceAsync(Guid variantId, decimal price, CancellationToken ct);

    /// <summary>
    /// The amounts of one item in the fixed order of 02 3.8.3: the unit price - the discounted one, already rounded, in a discount - times the quantity, the
    /// coupon share taken off, the VAT separated from the rest at the item's rate and rounded to the kuruş in the item, the net amount the rest. A pure calculation.
    /// </summary>
    ItemAmounts PriceItem(ItemPriceInput input);

    /// <summary>
    /// The shipping fee's VAT split over the rates of the order's physical items (02 3.19.2): one rate takes the whole fee; several share it in proportion to their
    /// items' amounts, the parts rounded down and the left-over kuruş on the largest part; each part's VAT at its rate. Empty for a fee of zero. A pure calculation.
    /// </summary>
    IReadOnlyList<ShippingVatPart> SplitShippingFee(decimal shippingFee, IReadOnlyList<ShippingBasisLine> lines);

    /// <summary>The order total (02 3.8.3; K-61): the sum of the items' payable amounts and the shipping fee, never net plus VAT again. A pure calculation.</summary>
    decimal SumOrderTotal(IReadOnlyList<ItemAmounts> items, decimal shippingFee);

    /// <summary>
    /// Adds a new coupon to the caller's unit of work (02 3.10.2, 3.10.4, 3.10.5; 07 UC-122): a percentage above zero and under one hundred, or a fixed amount above zero
    /// with a larger minimum basket amount; a date range; a usage count. Every field problem and a code another coupon holds - compared without letter case, "I", "İ",
    /// "ı" and "i" being one letter - refuse it, and nothing is added. Joins the caller's transaction and never saves (09 9.1 item 3).
    /// </summary>
    Task<CouponWriteResult> CreateCouponAsync(CouponDefinition definition, CancellationToken ct);

    /// <summary>
    /// The verdict on a coupon code for a basket now (02 3.10.1-3.10.5): accepted with every item's share - a percentage rounded in each item, a fixed amount spread in
    /// proportion with the left-over kuruş on the largest item - and the VAT separated after it; not accepted when unknown, outside its dates, its rights used up, equal
    /// to or above the basket, or rounded to a total of zero; its own outcome under the minimum basket amount. The basket total before the coupon is the base of the
    /// thresholds (02 3.10.7). A plain read; it counts no attempt and reserves no right - the confirmation reserves it (<see cref="ReserveAsync"/>).
    /// </summary>
    Task<CouponApplication> EvaluateCouponAsync(string? code, IReadOnlyList<CouponLine> lines, CancellationToken ct);

    /// <summary>
    /// The payment step's coupon field (07 UC-23; 02 3.10.5, 8.2 L-6): <see cref="EvaluateCouponAsync"/> behind the cart's invalid attempt limit - past the limit the field
    /// closes and the code is not looked at; a code that is not accepted counts toward the limit, an accepted one and one under its minimum basket amount do not. The
    /// attempt rows are written in the counter's own short transactions; nothing else is written.
    /// </summary>
    Task<CouponApplication> ApplyCouponAsync(Guid cartId, string? code, IReadOnlyList<CouponLine> lines, CancellationToken ct);

    /// <summary>
    /// Whether closing an order's units gives its coupon right back (02 5.11.3, 7.2.6, 7.4.8): only when every unit of every item is closed - cancelled, removed, terminated
    /// for late delivery, withdrawn or returned; a partial cancellation or return keeps the right. The right moves through <see cref="ReturnAsync"/>. A pure rule.
    /// </summary>
    bool ReturnsCouponRight(IReadOnlyList<ItemClosure> items);

    /// <summary>
    /// The whole category tree, depth first: a node, then its sub-categories in the Turkish alphabetical order of their names at every level (02 3.4.1) - the order of the panel's tree,
    /// the product form and the storefront menu. Each node carries its direct sub-category and product counts (02 3.4.5). A plain read.
    /// </summary>
    Task<IReadOnlyList<CategoryNode>> GetCategoryTreeAsync(CancellationToken ct);

    /// <summary>
    /// Creates a category at the root (<paramref name="parentCategoryId"/> empty) or under a parent (07 9.6.2; 02 3.4.1; 06 4.9): a blank name, a missing parent and a fourth level are refused. Takes the
    /// category tree's lock as the first statement of the caller's transaction, which it joins and must run inside (09 9.1 item 3; 05 7.3.5).
    /// </summary>
    Task<CategoryWriteResult> CreateCategoryAsync(string? name, Guid? parentCategoryId, CancellationToken ct);

    /// <summary>Renames a category (07 9.6.3): its address and level stay; its place among its siblings follows the new name. Joins the caller's transaction.</summary>
    Task<CategoryWriteResult> RenameCategoryAsync(Guid categoryId, string? name, CancellationToken ct);

    /// <summary>
    /// Moves a category with its whole branch under another parent or to the root (07 9.6.4; 02 3.4.6): refused when the move would pass three levels or put the category under its own
    /// branch, and then nothing is written. Takes the tree's lock first and joins the caller's transaction.
    /// </summary>
    Task<CategoryWriteResult> MoveCategoryAsync(Guid categoryId, Guid? newParentCategoryId, CancellationToken ct);

    /// <summary>
    /// Deletes a category that holds no product and no sub-category (07 9.6.5; 02 3.4.5); otherwise refuses with the two counts that block it. Takes the tree's lock first and joins the
    /// caller's transaction.
    /// </summary>
    Task<CategoryWriteResult> DeleteCategoryAsync(Guid categoryId, CancellationToken ct);

    /// <summary>
    /// Sets the categories a product hangs on and its main category (02 3.4.2, 3.4.4; 06 4.10): any node, any number, exactly one main category among them, or none when the set is empty.
    /// The product's version rises when the set changes (06 2.6.2). Joins the caller's transaction.
    /// </summary>
    Task<ProductCategoriesResult> SetProductCategoriesAsync(Guid productId, IReadOnlyCollection<Guid> categoryIds, Guid? primaryCategoryId, CancellationToken ct);

    /// <summary>
    /// A category page's product list (02 3.4.3, 3.5.2, 3.5.3; 07 2.6.3): the published products of the category and of every branch below it in one list, newest first, filtered on the price
    /// range and the stock state only, paged at the storefront page size (09 10.5 item 4). Null when no category has <paramref name="categorySlug"/>; an empty category is an empty page. A plain read.
    /// </summary>
    Task<ProductListPage?> ListCategoryProductsAsync(string? categorySlug, ProductListFilter? filter, int page, CancellationToken ct);

    /// <summary>
    /// The search result (02 3.5.1; 05 7.9; 07 2.6.3): the published products whose name contains the word - compared without letter case and Turkish characters on the simplified name; the
    /// description, the category name and the option values are not searched. Same order, page size and filters as the category page. Null when the word is empty or only blanks. A plain read.
    /// </summary>
    Task<ProductListPage?> SearchProductsAsync(string? search, ProductListFilter? filter, int page, CancellationToken ct);

    /// <summary>
    /// Saves a product's gallery as the product form posts it (02 3.11.2, 3.11.3; 06 4.71): the links are made to match the list, the position in the list is the order, the main mark is the
    /// listed one - or the current main image while it stays, or the first image - so a gallery that has an image always has exactly one main image, and the firm's alternative text is kept.
    /// The gallery's and the variants' images together, each once, may not pass P-24. A refusal writes nothing. Joins the caller's transaction and never saves it (09 9.1 item 3); outside one it throws.
    /// </summary>
    Task<GallerySaveResult> SaveProductGalleryAsync(Guid productId, IReadOnlyList<GalleryEntry> entries, CancellationToken ct);

    /// <summary>
    /// Gives a variant its own image or - with null - takes it away so that it takes the product's again (02 3.11.1); the variant's image counts toward P-24 (02 3.11.3). Joins the caller's transaction
    /// and never saves it; outside one it throws.
    /// </summary>
    Task<VariantImageResult> SetVariantImageAsync(Guid variantId, Guid? imageId, CancellationToken ct);

    /// <summary>The product's gallery in its order with the text each image uses (02 3.11.2, 3.11.4): the firm's when written, else "product name - image n / total". A plain read.</summary>
    Task<IReadOnlyList<GalleryImage>> GetProductGalleryAsync(Guid productId, CancellationToken ct);

    /// <summary>The alternative text of the image the showcase card and the list show (02 3.11.4): the firm's text for the main image when written, else the product's name alone; null for no such product. A plain read.</summary>
    Task<string?> GetCardImageAltTextAsync(Guid productId, CancellationToken ct);

    /// <summary>The image a variant shows (02 3.11.1, 3.11.4): its own, else the product's main image, with the text for it; null when it has neither. A plain read.</summary>
    Task<VariantImage?> GetVariantImageAsync(Guid variantId, CancellationToken ct);
}
