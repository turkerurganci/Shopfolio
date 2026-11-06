// Deliberate violation sample of the compiled cache rule (T229; 09 10.4 item 2, 20 item 9; 05 7.10.1): a type that reaches a cache library of the
// framework or caches a page's output. Compiled into its own assembly (CacheViolationSample.dll); the rule reads the compiled dependencies, so an alias
// or a fully qualified name that a text scan may not read as such is caught here.
using MC = Microsoft.Extensions.Caching.Distributed;

namespace Shopfolio.Modules.Catalog
{
    internal sealed class ForbiddenMemoryCacheUser
    {
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache? _cache = null;

        public bool Read(object key) => _cache is not null && _cache.TryGetValue(key, out _);
    }

    internal sealed class ForbiddenAliasedCacheUser
    {
        public MC.IDistributedCache? Cache { get; set; }
    }

    // 09 20 item 9: a static field of a mutable collection can gather values at run time - a cache with no invalidation rule. The read-only table,
    // the constant and the instance field are allowed.
    internal static class ForbiddenStaticCaches
    {
        public static System.Collections.Generic.IDictionary<string, object> Values = new System.Collections.Generic.Dictionary<string, object>();

        private static readonly System.Collections.Generic.Dictionary<string, object> _byKey = new();

        private static readonly System.Collections.Concurrent.ConcurrentBag<object> _bag = new();

        private static readonly System.Collections.Hashtable _hashtable = new();

        private static readonly System.Collections.Generic.List<object> _list = new();

        private static readonly System.Collections.Generic.HashSet<string> _set = new();

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, object> _weak = new();

        private static readonly System.Collections.Generic.IReadOnlyDictionary<string, int> _table = new System.Collections.Generic.Dictionary<string, int> { ["a"] = 1 };

        private const string Constant = "a";

        public static int Count => _byKey.Count + _bag.Count + _hashtable.Count + _list.Count + _set.Count + _table.Count + Constant.Length + (_weak.TryGetValue(Constant, out _) ? 1 : 0);
    }

    internal sealed class AllowedInstanceCollection
    {
        private readonly System.Collections.Generic.Dictionary<string, object> _values = new();

        public int Count => _values.Count;
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
