namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>The four kinds of record that have a page of their own (T505; 02 3.27.3-3.27.5, 3.27.9; 04 5.8): Hakkımızda at its fixed address, the others by <c>{Slug}</c>.</summary>
public enum ContentPageKind
{
    AboutPage,
    Offering,
    PortfolioItem,
    CustomPage,
}
