namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>The blocks of the home page (02 3.28.1-3.28.3; 04 5.1): four, and nothing else - no banner, no campaign picture, no free page building.</summary>
public enum HomepageBlockKind
{
    /// <summary>The corporate block: the about page's short introduction and main image; the fallback of an empty or unpublished about page is the brand's (02 3.27.14).</summary>
    CorporateIntro,

    Offerings,

    PortfolioItems,

    ProductShowcase,
}
