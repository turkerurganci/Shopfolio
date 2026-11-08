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
