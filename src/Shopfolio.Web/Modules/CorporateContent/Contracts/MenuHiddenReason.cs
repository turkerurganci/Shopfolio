namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>Why a menu item is not on the site now (04 9.14.4): the panel names the reason in the item's row.</summary>
public enum MenuHiddenReason
{
    /// <summary>The type has no published record (02 3.28.4) - a type that has only drafts, and an about page that is not published, are in this case.</summary>
    NoPublishedRecord,

    /// <summary>The catalog has no published product, so the products part has nothing to show (02 3.28.6).</summary>
    NoPublishedProduct,
}
