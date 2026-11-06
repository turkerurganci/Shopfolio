namespace Shopfolio.Infrastructure.SearchEngine;

/// <summary>
/// Whether a product can be bought now, as the structured data says it (05 3.7.2: "ürün (fiyat, stok durumu)"). The module decides it from the record
/// - the stock net of the reservations (04 5.4.6) - and hands the page a plain value; the search engine data code never reads stock.
/// </summary>
internal enum StockStatus
{
    InStock,
    OutOfStock,
}

/// <summary>
/// What the product's structured data is made of (05 3.7.2): every value comes from the record. <see cref="Price"/> is the current price in lira as the record
/// holds it, <see cref="Path"/> the product's own address (<c>/urun/{slug}</c>) and <see cref="ImagePaths"/> the paths of its images, the main one first.
/// </summary>
internal sealed record ProductData(
    string Name,
    string? Description,
    IReadOnlyList<string> ImagePaths,
    string? Sku,
    decimal Price,
    StockStatus Availability,
    string Path);

/// <summary>The firm's structured data (05 3.7.2): the brand name, the site's address, the logo and the contact values the record has. Nothing is typed for the search engines.</summary>
internal sealed record OrganizationData(
    string Name,
    string? LogoPath,
    string? Telephone,
    string? Email,
    IReadOnlyList<string> SameAs);

/// <summary>One step of a breadcrumb trail (02 3.4.4: made from the product's main category): the name shown and the step's own address.</summary>
internal sealed record BreadcrumbItem(string Name, string Path);

/// <summary>One question of the FAQ page and its answer, as the record holds it (the answer may be formatted text).</summary>
internal sealed record FaqEntry(string Question, string AnswerHtml);

/// <summary>A JSON-LD block ready to be written: the schema.org type it describes and its JSON text (HTML-safe: no <c>&lt;</c>, <c>&gt;</c> or <c>&amp;</c> can be in it).</summary>
internal sealed record JsonLdBlock(string Type, string Json);

/// <summary>
/// Everything a page tells the search engines in its head (05 3.7.2): the title, the description, the page's own address, the main image and the logo the
/// preview falls back to, the brand name, whether the page is closed to the search engines, and the JSON-LD blocks. A page builds it from the record and
/// hands it to <see cref="SearchEngineData.Head"/>; no field is typed by the firm.
/// </summary>
internal sealed record PageSearchData(
    string Title,
    string? Description,
    string Path,
    string? ImagePath,
    string? LogoPath,
    string? BrandName,
    bool Closed,
    IReadOnlyList<JsonLdBlock> StructuredData)
{
    /// <summary>The Open Graph type of the page; <c>website</c> unless the page says otherwise (a product page says <c>product</c>).</summary>
    public string OpenGraphType { get; init; } = "website";
}
