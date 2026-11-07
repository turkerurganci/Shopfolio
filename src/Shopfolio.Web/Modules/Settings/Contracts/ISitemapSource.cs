namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>The paths of the site map in the order they are written (T313; 05 3.7.3): an immutable read record, the value of the cache's one site map slot (09 10.4 item 1).</summary>
public sealed record SitemapView(IReadOnlyList<string> Paths);

/// <summary>
/// The site map's one reader (T313; 05 3.7.3, 7.10.1; 07 4.32.1): the paths of the published records of the Catalog and CorporateContent modules, taken through their
/// Contracts, with the fixed list pages, and kept in the cache's site map slot. A record that feeds the map names <c>CacheArea.Sitemap</c> in
/// <c>CacheInvalidations</c> when it is saved (09 10.4 item 3); the next read loads again. This is the display path - nothing counts on it for a calculation.
/// </summary>
public interface ISitemapSource
{
    Task<SitemapView> GetAsync(CancellationToken ct);
}
