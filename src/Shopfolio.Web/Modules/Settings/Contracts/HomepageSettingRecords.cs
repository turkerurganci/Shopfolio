using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>The six menu names the firm can change (02 3.28.4; 06 4.2): the names of the fixed skeleton's items. They also title the home page blocks (04 5.1.5).</summary>
public sealed record MenuNames(string Products, string Offerings, string PortfolioItems, string AboutPage, string FaqItems, string Contact);

/// <summary>The home page and menu section as stored (06 4.2): the chosen layout, the six menu names and the section's own version.</summary>
public sealed record HomepageSection(HomepageLayout Layout, MenuNames MenuNames, int Version);
