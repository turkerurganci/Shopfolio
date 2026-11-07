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
}
