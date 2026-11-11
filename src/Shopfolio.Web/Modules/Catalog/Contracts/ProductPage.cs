namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>A variant on the product page: the visitor sees only the Published ones; the administrator's preview adds the Draft ones, marked by <see cref="Status"/> (02 3.7.5).</summary>
public sealed record ProductPageVariant(Guid Id, string Sku, decimal Price, bool IsSoldOut, PublishStatus Status)
{
    /// <summary>The place of the variant's value in the product's first option (<c>OptionValue.SortOrder</c>; 07 4.4.1: the value of <c>firstOptionValue</c>); null when the product has no option.</summary>
    public int? FirstValueOrder { get; init; }

    /// <summary>The place of the variant's value in the product's second option (the value of <c>secondOptionValue</c>); null when the product has fewer than two options.</summary>
    public int? SecondValueOrder { get; init; }
}

/// <summary>One value of an option as the page's selection lists it: <see cref="Order"/> is <c>OptionValue.SortOrder</c>, the number the selection parameter carries (07 4.4.1; K-1178 (1)).</summary>
public sealed record ProductPageOptionValue(int Order, string Text);

/// <summary>One option dimension of the product (02 3.3.2): its name and its values in the firm's order. A product has at most two.</summary>
public sealed record ProductPageOption(string Name, IReadOnlyList<ProductPageOptionValue> Values);

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

    /// <summary>The product's type; decides the delivery line, the quantity field and the origin line (04 5.4.5, 5.4.7).</summary>
    public ProductType Type { get; init; }

    /// <summary>The product's address (07 4.4.1).</summary>
    public string? Slug { get; init; }

    /// <summary>The product's own shipping period in business days; null when it sets none and the firm default applies (02 3.20.5).</summary>
    public int? ShippingPeriod { get; init; }

    /// <summary>A service's performance period in days from the payment's confirmation (02 3.2.2); null when none.</summary>
    public int? FulfilmentPeriod { get; init; }

    /// <summary>The product's primary category (02 3.4.4): the breadcrumb's source; null when none.</summary>
    public Guid? PrimaryCategoryId { get; init; }

    /// <summary>The option dimensions with their values, in the firm's order; empty for a product without options (02 3.3.1).</summary>
    public IReadOnlyList<ProductPageOption> Options { get; init; } = [];

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
