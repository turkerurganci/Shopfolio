using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>The six menu names the firm can change (02 3.28.4; 06 4.2): the names of the fixed skeleton's items. They also title the home page blocks (04 5.1.5).</summary>
public sealed record MenuNames(string Products, string Offerings, string PortfolioItems, string AboutPage, string FaqItems, string Contact);

/// <summary>The home page and menu section as stored (06 4.2): the chosen layout, the six menu names and the section's own version.</summary>
public sealed record HomepageSection(HomepageLayout Layout, MenuNames MenuNames, int Version);

/// <summary>The product's default menu names (02 3.28.4; K-813): they come with the install and are what an empty name field reminds the firm of.</summary>
public static class MenuNameDefaults
{
    public const string Products = "Ürünler";

    public const string Offerings = "Hizmetlerimiz";

    public const string PortfolioItems = "Referanslarımız";

    public const string AboutPage = "Hakkımızda";

    public const string FaqItems = "SSS";

    public const string Contact = "İletişim";
}

/// <summary>
/// The values the home page and menu form sends (07 9.14.2 <c>SaveHomepageInput</c>; 06 4.2) without the version, which travels apart. The social links are written by the content module in the same
/// transaction (06 4.55); <paramref name="SocialLinkChanges"/> are the rows it changed, so the section's version steps and the trail row carries them.
/// </summary>
public sealed record HomepageInput(
    HomepageLayout? HomepageLayout,
    string? MenuNameProducts,
    string? MenuNameOfferings,
    string? MenuNamePortfolioItems,
    string? MenuNameAboutPage,
    string? MenuNameFaqItems,
    string? MenuNameContact,
    string? WhatsAppNumber,
    IReadOnlyList<SocialLinkChange>? SocialLinkChanges = null);
