using Shopfolio.Modules.Catalog.Contracts.Categories;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>The items of the menu's fixed skeleton (02 3.28.4, 3.28.5): the five that can be hidden, the general pages the firm shows in the menu, and the contact page that is always there.</summary>
public enum MenuItemKind
{
    Products,
    Offerings,
    PortfolioItems,
    AboutPage,
    FaqItems,
    GeneralPage,
    Contact,
}

/// <summary>Why a menu item is not on the site now (04 9.14.4): the panel names the reason in the item's row.</summary>
public enum MenuHiddenReason
{
    /// <summary>The type has no published record (02 3.28.4) - a type that has only drafts, and an about page that is not published, are in this case.</summary>
    NoPublishedRecord,

    /// <summary>The catalog has no published product, so the products part has nothing to show (02 3.28.6).</summary>
    NoPublishedProduct,
}

/// <summary>
/// One item of the top menu. <see cref="Name"/> is the firm's name for the item (the product's default until it changes it) or, for a general page, its title; <see cref="Path"/> is the
/// page the item opens, or null for the products item, which only opens the category list (02 3.28.4; 04 5.1.6 - there is no all-products page). A hidden item has its reason.
/// </summary>
public sealed record MenuItem(MenuItemKind Kind, string Name, string? Path, bool IsVisible, MenuHiddenReason? HiddenReason);

/// <summary>A published general page as the menu and the footer list it (02 3.28.5): its title and address, and whether the firm shows it in the top menu as well.</summary>
public sealed record GeneralPageLink(Guid Id, string Title, string Path, bool IsInMenu);

/// <summary>
/// The storefront's menu (T361; 02 3.28.4-3.28.6; 04 2.2.3): <see cref="Items"/> are all the skeleton's items - the hidden ones too, each with its reason, for the panel (04 9.14.4) - in
/// the order the layout gives (content-first: the content types, "Ürünler", the menu's general pages, İletişim; store-first: "Ürünler" first), and the storefront draws the ones that
/// <see cref="MenuItem.IsVisible"/>. <see cref="Categories"/> are the categories under "Ürünler" - those holding a published product in themselves or below - depth first in alphabetical
/// order, and empty while the products part is hidden. <see cref="FooterPages"/> are all published general pages in the firm's order (02 3.28.5). There is no way to add, remove or
/// reorder an item (02 3.28.4).
/// </summary>
public sealed record StorefrontMenu(
    HomepageLayout Layout,
    IReadOnlyList<MenuItem> Items,
    IReadOnlyList<MenuCategory> Categories,
    IReadOnlyList<GeneralPageLink> FooterPages)
{
    /// <summary>The items the storefront draws, in order.</summary>
    public IEnumerable<MenuItem> VisibleItems => Items.Where(i => i.IsVisible);
}
