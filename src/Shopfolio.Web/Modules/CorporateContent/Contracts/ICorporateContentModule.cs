namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// The open surface of the CorporateContent module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the corporate pages, branding and contact requests (05 3.3.7).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface ICorporateContentModule
{
    /// <summary>
    /// The page addresses the site map takes from this module (05 3.7.3; 02 3.30.5, 3.30.6): <c>/hizmet/{slug}</c>, <c>/referans/{slug}</c> and
    /// <c>/sayfa/{slug}</c> of every published record, and <c>/hakkimizda</c> when the about page is published. A draft is not listed. A plain read.
    /// </summary>
    Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct);

    /// <summary>
    /// The storefront's menu (T361; 02 3.28.4-3.28.6; 04 2.2.3): the fixed skeleton in the layout's order with the firm's names, each item with its visibility and - when hidden - its reason;
    /// the categories under "Ürünler"; the published general pages for the footer and the menu. A type with no published record is hidden, a category with no published product below it is
    /// not listed. The structure is cached; the names and the layout are read on every call. A plain read.
    /// </summary>
    Task<StorefrontMenu> GetMenuAsync(CancellationToken ct);

    /// <summary>
    /// The home page (T361; 02 3.28.1-3.28.3; 04 5.1): the blocks that show in the order of the chosen layout - the corporate block, the flagged services and references, the newest
    /// published products - each with what it shows. A block with nothing to show is not there; a draft is never on the page. The corporate part is cached; the showcase is read on every
    /// call. A plain read.
    /// </summary>
    Task<HomepageView> GetHomepageAsync(CancellationToken ct);
}
