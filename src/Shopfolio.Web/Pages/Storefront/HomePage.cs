using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Storefront;

/// <summary>The corporate block's content (04 5.1.3, 5.1.4): the about page's short introduction, its main image when it has one and the page's address.</summary>
public sealed record HomeIntro(string ShortIntroduction, string Path, CardImage? Image);

/// <summary>A block of content cards (04 5.1.5): the firm's menu name as the title, the list page it links to, at most three cards and the "Tümünü gör" link when more records are flagged.</summary>
public sealed record HomeCardBlock(string Title, string ListPath, IReadOnlyList<ContentCard> Cards, bool HasMore);

/// <summary>A first-level category link under the showcase (04 5.1.6; K-805).</summary>
public sealed record HomeCategoryLink(string Name, string Path);

/// <summary>The product showcase (04 5.1.6): the newest published products as product cards and, under them, the first-level categories that hold a published product.</summary>
public sealed record HomeShowcase(string Title, IReadOnlyList<ProductCard> Cards, IReadOnlyList<HomeCategoryLink> CategoryLinks);

/// <summary>
/// The home page as the screen draws it (E-01; 04 5.1; 07 UC-01): the blocks in the order of the firm's layout, each with the model of the component that draws it. The corporate block is always
/// there; when <see cref="Intro"/> is null the screen shows the brand name and logo instead (04 5.1.11). It is made from the module's view in one place so the draw has nothing to decide.
/// </summary>
public sealed record HomePage(
    HomepageLayout Layout,
    IReadOnlyList<HomepageBlockKind> Blocks,
    HomeIntro? Intro,
    HomeCardBlock? Offerings,
    HomeCardBlock? PortfolioItems,
    HomeShowcase? Showcase)
{
    /// <summary>The text of the corporate block's image when the firm wrote none for it (02 3.11.4: never empty).</summary>
    public const string IntroImageFallbackText = "Hakkımızda";

    /// <summary>The page of a site with nothing yet: the corporate block alone, with the brand's fallback (04 5.1.11); the value before the handler has read the module.</summary>
    public static HomePage Empty { get; } = new(HomepageLayout.ContentFirst, [HomepageBlockKind.CorporateIntro], null, null, null, null);

    /// <summary>The corporate block is wide in the content-first layout and a narrow band in the store-first one (04 5.1.3, 5.1.4).</summary>
    public bool IsCorporateBlockWide => Layout != HomepageLayout.StoreFirst;

    /// <summary>The rows the showcase draws on the wide class: one in the content-first layout, two in the store-first one (04 5.1.3, 5.1.4).</summary>
    public int ShowcaseRows => Layout == HomepageLayout.StoreFirst ? 2 : 1;

    /// <summary>The screen's model of the module's home page view.</summary>
    public static HomePage Of(HomepageView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        return new HomePage(
            view.Layout,
            view.Blocks,
            view.Intro is { } intro
                ? new HomeIntro(
                    intro.ShortIntroduction,
                    intro.Path,
                    intro.MainImage is { } image
                        ? ProductCards.ImageOf(image.ImageId, string.IsNullOrWhiteSpace(image.AltText) ? IntroImageFallbackText : image.AltText.Trim())
                        : null)
                : null,
            CardBlockOf(view.Offerings),
            CardBlockOf(view.PortfolioItems),
            view.Showcase is { } showcase
                ? new HomeShowcase(
                    showcase.Title,
                    ProductCards.Of(showcase.Products),
                    [.. showcase.CategoryLinks.Select(c => new HomeCategoryLink(c.Name, AddressRules.RecordAddress(AddressRules.CategoryPrefix, c.Slug)))])
                : null);
    }

    private static HomeCardBlock? CardBlockOf(HomepageCardBlock? block) =>
        block is null ? null : new HomeCardBlock(block.Title, block.ListPath, ContentCards.Of(block.Cards), block.HasMore);
}
