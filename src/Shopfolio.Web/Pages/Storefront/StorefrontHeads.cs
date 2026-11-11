using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// What the storefront's list and home screens tell the search engines (05 3.7.2: in the first response; 02 3.30.5), made from the frame's brand and the record's values by the one pattern of
/// <see cref="RecordPages"/> and <see cref="SearchEngineData"/>. The firm types none of it. The screens write the result with <see cref="SearchEngineData.Head"/> in their head section.
/// </summary>
internal static class StorefrontHeads
{
    /// <summary>
    /// The home page (E-01): the brand name as the title, the short introduction as the description when the about page has one, and the firm's structured data - the brand name, the logo, the
    /// telephone and e-mail of the identity set and the social links. The page stays as it is while sales are closed: nothing here reads the sales gate (04 5.1.10).
    /// </summary>
    public static PageSearchData Home(SearchEngineData search, StorefrontFrame frame, HomePage page)
    {
        ArgumentNullException.ThrowIfNull(search);
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(page);

        FrameBrand brand = frame.Brand;
        string? logoPath = LogoPathOf(brand);
        JsonLdBlock firm = search.Organization(new OrganizationData(
            brand.BrandName,
            logoPath,
            frame.Footer.Identity.Phone,
            frame.Footer.Identity.Email,
            [.. frame.Contact.SocialLinks.Select(link => link.Url)]));

        return new PageSearchData(
            brand.BrandName,
            SearchText.Description(page.Intro?.ShortIntroduction, null),
            "/",
            page.Intro?.Image?.Path,
            logoPath,
            brand.BrandName,
            false,
            [firm]);
    }

    /// <summary>
    /// A category page (E-02; 02 3.30.5 item 2): the name and the brand name, the generated description, the breadcrumb trail as structured data. A page that carries a filter - any parameter other
    /// than <c>page</c> - is closed (07 2.6.7); the closed mark is the list pattern's, not computed here.
    /// </summary>
    public static PageSearchData Category(SearchEngineData search, StorefrontFrame frame, string name, string slug, IReadOnlyList<CategoryCrumb> trail, bool closed)
    {
        ArgumentNullException.ThrowIfNull(search);
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(trail);

        List<BreadcrumbItem> steps = [new("Ana sayfa", "/")];
        steps.AddRange(trail.Select(c => new BreadcrumbItem(c.Name, c.Path)));
        JsonLdBlock? crumbs = search.Breadcrumbs(steps);
        PageSearchData page = RecordPages.Category(name, slug, null, LogoPathOf(frame.Brand), frame.Brand.BrandName, crumbs is null ? [] : [crumbs]);
        return page with { Closed = closed };
    }

    private static string? LogoPathOf(FrameBrand brand) => brand.LogoId is Guid logoId ? FrameStyle.ImagePath(logoId) : null;
}

/// <summary>One step of a category's path (04 5.2.3): the category's name and its page's address; the last step is the page itself.</summary>
public sealed record CategoryCrumb(string Name, string Path);

/// <summary>The category page's address (07 2.1.2): <c>/kategori/{slug}</c>, written once.</summary>
internal static class CategoryPaths
{
    public static string Of(string slug) => AddressRules.RecordAddress(AddressRules.CategoryPrefix, slug);
}
