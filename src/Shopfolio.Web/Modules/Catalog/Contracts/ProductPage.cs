namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>What an address under <c>/urun/{slug}</c> answers (02 3.7.5, 3.7.6, 3.30.4): the page, the "not sold any more" page, the administrator's draft preview, or "page not found".</summary>
public enum ProductPageKind
{
    /// <summary>No page: the address is unknown, deleted or a draft's seen by a visitor - one and the same "page not found" (02 3.30.4).</summary>
    NotFound,

    /// <summary>A Published product: the product page with its Published variants.</summary>
    Published,

    /// <summary>A Draft product seen by a signed-in administrator: the page with the "Draft" band and every variant but the archived (02 3.7.5).</summary>
    DraftPreview,

    /// <summary>An Archived product: the "Bu ürün artık satılmıyor" page - name, image and status, no price and no cart (02 3.7.6).</summary>
    Archived,
}

/// <summary>A variant on the product page: the visitor sees only the Published ones; the administrator's preview adds the Draft ones, marked by <see cref="Status"/> (02 3.7.5).</summary>
public sealed record ProductPageVariant(Guid Id, string Sku, decimal Price, bool IsSoldOut, PublishStatus Status);

/// <summary>
/// The product page the storefront shows for an address (T335; 02 3.7.5, 3.7.6, 3.30.4, 3.30.6): what the page carries and what it does not. An archived page has no variants, so no
/// price and no add-to-cart; a draft shows to an administrator only, with its band; everything else is "page not found". The page's wording and markup are the page's; this record
/// says what is shown.
/// </summary>
public sealed record ProductPage(
    ProductPageKind Kind,
    Guid? ProductId,
    string? Name,
    string? Description,
    Guid? MainImageId,
    IReadOnlyList<ProductPageVariant> Variants)
{
    /// <summary>No page; the answer of an unknown, deleted or draft-for-a-visitor address.</summary>
    public static ProductPage NotFound { get; } = new(ProductPageKind.NotFound, null, null, null, null, []);

    /// <summary>The "Draft" band of an administrator's preview (02 3.7.5).</summary>
    public bool ShowsDraftBand => Kind == ProductPageKind.DraftPreview;

    /// <summary>The page names prices: a product page and the draft preview do; the archived page and "not found" do not (02 3.7.6).</summary>
    public bool ShowsPrice => Kind is ProductPageKind.Published or ProductPageKind.DraftPreview;

    /// <summary>The page offers the cart: only a Published product's page does - a preview shows the page, it does not sell (02 3.7.5, 3.7.6).</summary>
    public bool OffersCart => Kind == ProductPageKind.Published;

    /// <summary>The search engines may take the page: only a Published product's page; the archived page is closed to them (02 3.7.6, 3.30.6).</summary>
    public bool IsIndexable => Kind == ProductPageKind.Published;

    /// <summary>The page is the "Bu ürün artık satılmıyor" page (02 3.7.6).</summary>
    public bool IsNoLongerSold => Kind == ProductPageKind.Archived;
}
