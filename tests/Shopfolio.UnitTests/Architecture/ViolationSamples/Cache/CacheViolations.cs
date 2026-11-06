// Deliberate violation sample of the cache source scan (09 10.4 items 2, 3 and 6, 20 item 9; 05 7.10.1): a cache outside its one place.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class AppCache { }
internal enum CacheArea { Menu }
internal sealed record class CacheInvalidations();
internal sealed class StrayWriter(AppCache cache, CacheInvalidations invalidations)
{
    public void Drop() => cache.Invalidate(CacheArea.Settings);
    public void Maybe(AppCache? other) => other?.Invalidate(CacheArea.Menu);
    public void Group(IEnumerable<CacheArea> areas) { foreach (CacheArea a in areas) Run(cache.Invalidate, a); }
    public string Named() => nameof(AppCache.Invalidate);
    public object? Reflected() => typeof(AppCache).GetMethod("Invalidate");
    public void Forget() => invalidations.TakePendingAreas();
    private static void Run(Action<CacheArea> act, CacheArea area) => act(area);
}
internal sealed class StrayCache(IMemoryCache memory, IDistributedCache distributed, HybridCache hybrid)
{
    private static readonly ConcurrentDictionary<string, object> _values = new();
    private static readonly Lazy<string> _lazy = new(() => "x");
    private static readonly System.Lazy<int> _qualified = new(() => 1);
    public void Register(IServiceCollection services) => services.AddMemoryCache();
    public void Distributed(IServiceCollection services) => services.AddStackExchangeRedisCache(_ => { });
    public void Initialize(ref string? value) => LazyInitializer.EnsureInitialized(ref value, () => "x");
    public System.Runtime.Caching.ObjectCache Old() => System.Runtime.Caching.MemoryCache.Default;
    public Lazy<BusinessCalendar> NotTheCalendarFile() => new(() => BusinessCalendar.Product);
}
[OutputCache(Duration = 60)]
internal sealed class CachedPage : PageModel { }
[ResponseCache(Duration = 60)]
internal sealed class CachedHeaders : PageModel { }
internal static class Pipeline
{
    public static void Add(IServiceCollection services) => services.AddOutputCache();
    public static void Use(WebApplication app) { app.UseOutputCache(); app.UseResponseCaching(); }
    public static void Map(WebApplication app) => app.MapGet("/", () => "x").CacheOutput();
    public static void Store(IOutputCacheStore store) => _ = store;
}

// Not findings: a comment that says cache.Invalidate(CacheArea.Menu) and new MemoryCache(); words that only contain the names.
internal sealed class Clean
{
    public DateTimeOffset? InvalidatedAt { get; set; }
    public string Note => "AppCacheArea";
    public IReadOnlyList<CacheArea> Areas { get; } = [];
}
