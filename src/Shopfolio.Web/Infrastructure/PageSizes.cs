namespace Shopfolio.Infrastructure;

/// <summary>
/// The page sizes of the lists (09 10.5 item 4): the numbers of the layout in 04 2.13.3, written here once. A page size is not a request parameter and
/// not a setting (07 2.6.2). The groups of the corporate content that the firm orders by hand are not paged (04 9.13.3).
/// </summary>
internal static class PageSizes
{
    /// <summary>The product lists of the storefront (04 2.13.3; K-766).</summary>
    public const int StorefrontProducts = 24;

    /// <summary>The content list page, E-07 (04 2.13.3; K-851).</summary>
    public const int ContentList = 24;

    /// <summary>The member's order history, E-27 (04 2.13.3; K-784).</summary>
    public const int OrderHistory = 25;

    /// <summary>The lists of the panel (04 2.13.3; K-766).</summary>
    public const int Panel = 25;
}
