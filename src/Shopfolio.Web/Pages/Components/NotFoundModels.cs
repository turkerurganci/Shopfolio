using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The view model of the "page not found" page E-06 (T504; 04 5.6; 07 UC-06): the way home and the way to the products. The words are the page's own and the firm does not edit them (04 5.6.3;
/// <c>02 3.30.4</c>); nothing here says why the address was not found (04 5.6.4) and nothing is a code, an id or a technical detail (07 2.7.3).
/// </summary>
/// <param name="HomePath">The way home (<see cref="NotFoundTexts.HomeLabel"/>).</param>
/// <param name="ProductLinks">The first-level categories of the menu - the links the home page shows; empty while no product is published, and the products way then does not show (04 5.6.3; 2.7.1.1).</param>
public sealed record NotFoundPageView(string HomePath, IReadOnlyList<MenuCategoryNode> ProductLinks);

/// <summary>The fixed words of the "page not found" page E-06 (04 5.6.2, 5.6.3); the title says the page was not found and it is not the unexpected error page's title (04 5.6.8).</summary>
public static class NotFoundTexts
{
    /// <summary>The title of the page (04 5.6.2).</summary>
    public const string Title = "Sayfa bulunamadı";

    /// <summary>The one short sentence under the title; it does not say whether the address never existed or is closed to the visitor (04 5.6.4).</summary>
    public const string Message = "Aradığınız sayfa bulunamadı.";

    /// <summary>The label of the way home (04 5.6.3 item 2).</summary>
    public const string HomeLabel = "Ana sayfaya dön";

    /// <summary>The heading of the way to the products (04 5.6.3 item 3).</summary>
    public const string ProductsLabel = "Ürünlere dön";
}
