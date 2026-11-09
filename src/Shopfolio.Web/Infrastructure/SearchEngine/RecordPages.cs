using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Infrastructure.SearchEngine;

/// <summary>
/// The search engine data of a record page, made from the record's own values by one set of rules (T363; 02 3.30.5, 3.30.6; 05 3.7.2): the title is the name and the brand name,
/// the description is the short field - or the start of the plain text, cut at a word - and the page's address is <c>/{prefix}/{slug}</c>; the preview image is the page's main image
/// and, when it has none, the logo (<see cref="SearchEngineData.Head"/> makes the fall-back). A draft and an archived product are closed (the page is not open) - they
/// carry the robots meta tag and nothing else for the search engines to read; the site map leaves them out in its own readers (T313). The firm types none of it: there is no
/// parameter a form could fill, the values are the record's.
/// </summary>
internal static class RecordPages
{
    /// <summary>The Open Graph type of a product page (05 3.7.2); every other record page is a <c>website</c>.</summary>
    public const string ProductOpenGraphType = "product";

    /// <summary>
    /// A product page. <paramref name="open"/> is true only for a published product: a draft (a visitor never sees it, an administrator's preview must not be indexed) and an
    /// archived product's page (02 3.7.6, 3.30.6: it opens by the direct link and stays out of the search results and the site map) are closed.
    /// </summary>
    public static PageSearchData Product(string name, string? shortText, string? descriptionHtml, string slug, string? imagePath, string? logoPath, string? brandName, bool open, IReadOnlyList<JsonLdBlock> structuredData) =>
        Record(AddressRules.ProductPrefix, name, shortText, descriptionHtml, slug, imagePath, logoPath, brandName, open, structuredData);

    /// <summary>A category page (02 3.30.5 item 2): the description is made from the category's name and the brand name; a category has no draft, so the page is open.</summary>
    public static PageSearchData Category(string name, string slug, string? imagePath, string? logoPath, string? brandName, IReadOnlyList<JsonLdBlock> structuredData)
    {
        ArgumentNullException.ThrowIfNull(structuredData);
        return new PageSearchData(
            SearchText.Title(name, brandName),
            SearchText.CategoryDescription(name, brandName),
            AddressRules.RecordAddress(AddressRules.CategoryPrefix, slug),
            imagePath,
            logoPath,
            brandName,
            false,
            structuredData);
    }

    /// <summary>
    /// A corporate content record's page - <paramref name="prefix"/> is the service, reference or general page prefix (02 3.27): the short introduction or description when the record has
    /// one, the start of its text otherwise. <paramref name="open"/> is true for a published record; a draft is closed.
    /// </summary>
    public static PageSearchData Content(string prefix, string name, string? shortText, string? textHtml, string slug, string? imagePath, string? logoPath, string? brandName, bool open, IReadOnlyList<JsonLdBlock> structuredData) =>
        Record(prefix, name, shortText, textHtml, slug, imagePath, logoPath, brandName, open, structuredData);

    /// <summary>
    /// The head data of a screen the search engines may not index - the cart, a payment step, the order tracking, the account and the panel (02 3.30.5 item 1; K-1559): the tab title is the
    /// screen's name and the brand name, the page is closed and carries no description, image or structured data.
    /// </summary>
    public static PageSearchData ClosedScreen(string screenName, string? brandName, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new PageSearchData(SearchText.Title(screenName, brandName), null, path, null, null, brandName, true, []);
    }

    private static PageSearchData Record(
        string prefix,
        string name,
        string? shortText,
        string? bodyHtml,
        string slug,
        string? imagePath,
        string? logoPath,
        string? brandName,
        bool open,
        IReadOnlyList<JsonLdBlock> structuredData)
    {
        ArgumentNullException.ThrowIfNull(structuredData);
        PageSearchData page = new(
            SearchText.Title(name, brandName),
            SearchText.Description(shortText, bodyHtml),
            AddressRules.RecordAddress(prefix, slug),
            imagePath,
            logoPath,
            brandName,
            !open,
            structuredData);
        return string.Equals(prefix, AddressRules.ProductPrefix, StringComparison.Ordinal) ? page with { OpenGraphType = ProductOpenGraphType } : page;
    }
}
