using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>The main image of a record as the home page shows it: the gallery's image and the firm's alternative text when it wrote one (02 3.11.4).</summary>
public sealed record ContentImage(Guid ImageId, string? AltText);

/// <summary>The corporate block of a published about page (02 3.28.2): the short introduction, the main image when the gallery has one, and the about page's address; the long text stays on its own page.</summary>
public sealed record HomepageIntro(string ShortIntroduction, string Path, ContentImage? MainImage);

/// <summary>The card of a flagged service introduction or reference work (04 5.1.5): title, short description, main image and the record's address.</summary>
public sealed record HomepageCard(Guid Id, string Title, string? ShortDescription, string Path, ContentImage? MainImage);

/// <summary>
/// A block of flagged records (02 3.28.3): <see cref="Title"/> is the firm's menu name, <see cref="ListPath"/> the list page of the type, <see cref="Cards"/> the first records of the firm's
/// manual order, and <see cref="HasMore"/> says there are more flagged records than cards - "Tümünü gör" leads to the list (04 5.1.5; K-755).
/// </summary>
public sealed record HomepageCardBlock(string Title, string ListPath, IReadOnlyList<HomepageCard> Cards, bool HasMore);

/// <summary>
/// The product showcase (02 3.28.1; 04 5.1.6): the newest published products as many as the layout shows - the firm picks none and no product carries a home page mark - and, under it,
/// the first-level categories that hold a published product, alphabetically. A sold-out product that comes up in the order is shown and marked.
/// </summary>
public sealed record HomepageShowcase(string Title, IReadOnlyList<ShowcaseProduct> Products, IReadOnlyList<MenuCategory> CategoryLinks);

/// <summary>
/// The home page (T361; 02 3.28.1-3.28.3): <see cref="Blocks"/> are the blocks that show, in the order the layout gives - content-first: the corporate block, services, references, the
/// showcase; store-first: the showcase, the corporate block, services, references. A block without anything to show is not in the list and its member is null: the services and the
/// references block when no published record is flagged, the showcase when no product is published. The corporate block is always in the list; <see cref="Intro"/> is null when the about
/// page is empty or not published, and the page then shows the brand name and logo instead (02 3.27.14, 3.27.22).
/// </summary>
public sealed record HomepageView(
    HomepageLayout Layout,
    IReadOnlyList<HomepageBlockKind> Blocks,
    HomepageIntro? Intro,
    HomepageCardBlock? Offerings,
    HomepageCardBlock? PortfolioItems,
    HomepageShowcase? Showcase);
