// Deliberate violation sample of the compiled cache rule (T229; 09 10.4 item 2, 20 item 9; 05 7.10.1): a type that reaches a cache library of the
// framework or caches a page's output. Compiled into its own assembly (CacheViolationSample.dll); the rule reads the compiled dependencies, so an alias
// or a fully qualified name that a text scan may not read as such is caught here.
using MC = Microsoft.Extensions.Caching.Distributed;

namespace Shopfolio.Modules.Catalog
{
    internal sealed class ForbiddenMemoryCacheUser
    {
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache? _cache = null;

        public object? Read(object key) => _cache?.Get(key);
    }

    internal sealed class ForbiddenAliasedCacheUser
    {
        public MC.IDistributedCache? Cache { get; set; }
    }
}

namespace Shopfolio.Pages.Storefront
{
    [Microsoft.AspNetCore.OutputCaching.OutputCache(Duration = 60)]
    internal sealed class ForbiddenOutputCachedPage
    {
    }

    [Microsoft.AspNetCore.Mvc.ResponseCache(Duration = 60)]
    internal sealed class ForbiddenResponseCachedPage
    {
    }
}

namespace Shopfolio.Infrastructure
{
    internal sealed class AllowedPlainType
    {
        public int Value { get; set; }
    }
}
