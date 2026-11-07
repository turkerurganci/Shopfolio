using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The site map's cache owner (T313; 05 3.7.3, 7.10.1; 09 10.4): the cache is reached here, inside a module, never from the endpoint. The load takes the two modules'
/// paths through their Contracts - in this scope's one context, one after the other - and composes them with the fixed list pages (<see cref="SiteMapFiles.Compose"/>).
/// </summary>
internal sealed class SitemapSource : ISitemapSource
{
    private readonly AppCache _cache;
    private readonly ICatalogModule _catalog;
    private readonly ICorporateContentModule _content;

    public SitemapSource(AppCache cache, ICatalogModule catalog, ICorporateContentModule content)
    {
        _cache = cache;
        _catalog = catalog;
        _content = content;
    }

    public Task<SitemapView> GetAsync(CancellationToken ct) => _cache.GetOrLoadAsync(CacheArea.Sitemap, LoadAsync, ct);

    private async Task<SitemapView> LoadAsync(CancellationToken ct)
    {
        IReadOnlyList<string> catalogPaths = await _catalog.ListSitemapPathsAsync(ct);
        IReadOnlyList<string> contentPaths = await _content.ListSitemapPathsAsync(ct);
        return new SitemapView(SiteMapFiles.Compose(catalogPaths, contentPaths));
    }
}
