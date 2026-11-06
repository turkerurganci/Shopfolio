using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the in-memory cache's single places (T229; 09 10.4 items 2, 3 and 6, 20 item 9; 05 7.10.1; MK-34). Four rules over the application's
/// C# and Razor files, and one over its dependency and deployment files:
/// <list type="number">
/// <item><c>AppCache</c>, <c>CacheArea</c> and <c>CacheInvalidations</c> are declared only in their own files under <c>Infrastructure/</c>.</item>
/// <item>An area is dropped only by the transaction runner after the commit: the word <c>Invalidate</c> (a call, a conditional call, a method group,
/// a <c>nameof</c>, a reflection string - every form names it) stands only in <c>TransactionRunner.cs</c> and in <c>AppCache.cs</c>, which defines it;
/// <c>TakePendingAreas</c> only in <c>TransactionRunner.cs</c> and in <c>CacheInvalidations.cs</c>, which defines it.</item>
/// <item>No other in-memory cache: the framework's memory, distributed and hybrid caches, <c>System.Runtime.Caching</c>, a <c>ConcurrentDictionary</c>,
/// <c>Lazy&lt;T&gt;</c> and <c>LazyInitializer</c>, and the Razor <c>&lt;cache&gt;</c> and <c>&lt;distributed-cache&gt;</c> tag helpers stand nowhere. The one
/// named exception is <c>Lazy&lt;BusinessCalendar&gt;</c> in <c>BusinessCalendar.cs</c>: it holds the holiday file that ships inside the application
/// (an embedded resource that cannot change while the process runs), not a record, so it has nothing to invalidate.</item>
/// <item>A page's output is not cached: output caching and response caching (the services, the middleware, the attributes, <c>CacheOutput</c>) stand nowhere.</item>
/// <item>The dependencies and the deployment files name no separate cache server or cache client (Redis, Valkey, Memcached and the like) and no cache
/// package of the framework's caching family.</item>
/// </list>
/// The places are full paths, not file names: a file of the same name elsewhere is scanned. Full-line comments are blanked first and keep their line.
/// </summary>
internal static partial class CacheSourceScan
{
    public const string DefinitionText = "declares a cache type outside its own file in Infrastructure/ (09 10.4 item 6)";

    public const string InvalidateText = "drops a cache area outside the transaction runner (09 10.4 items 3, 6)";

    public const string OtherCacheText = "keeps an in-memory cache outside AppCache (09 20 item 9, 10.4 item 2)";

    public const string PageOutputText = "caches a page's output (05 7.10.1; 09 10.4)";

    public const string DependencyText = "names a separate cache server or cache client in the dependencies (05 7.10.1; MK-34)";

    public const string InfrastructureFolder = "src/Shopfolio.Web/Infrastructure/";

    public const string AppCacheFile = "src/Shopfolio.Web/Infrastructure/AppCache.cs";

    public const string CacheAreaFile = "src/Shopfolio.Web/Infrastructure/CacheArea.cs";

    public const string CacheInvalidationsFile = "src/Shopfolio.Web/Infrastructure/CacheInvalidations.cs";

    public const string RunnerFile = "src/Shopfolio.Web/Infrastructure/TransactionRunner.cs";

    public const string BusinessCalendarFile = "src/Shopfolio.Web/Infrastructure/BusinessCalendar.cs";

    /// <summary>Each cache type and the one file that may declare it.</summary>
    public static readonly IReadOnlyDictionary<string, string> DefinitionFiles = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["AppCache"] = AppCacheFile,
        ["CacheArea"] = CacheAreaFile,
        ["CacheInvalidations"] = CacheInvalidationsFile,
    };

    /// <summary>The names the dependency rule refuses; the pattern is built from this list, and the test shows that each name goes red on a sample line.</summary>
    public static readonly IReadOnlyList<string> DependencyNames =
    [
        "redis", "valkey", "memcached", "enyim", "garnet", "keydb", "dragonfly", "ncache", "hazelcast", "microsoft.extensions.caching",
        "system.runtime.caching", "fusioncache", "easycaching", "cachemanager", "lazycache", "cachetower", "foundatio", "souin", "cache-handler",
    ];

    private static readonly Regex _dependency = new(
        "(?<![A-Za-z])(" + string.Join('|', DependencyNames.Select(Regex.Escape)) + ")(?![A-Za-z])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5));

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".cshtml", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        foreach (Match match in Definition().Matches(clean))
        {
            if (DefinitionFiles[match.Groups["name"].Value] != path)
            {
                AddAt(findings, DefinitionText, clean, match.Index);
            }
        }

        if (path != RunnerFile && path != AppCacheFile)
        {
            Add(findings, InvalidateText, InvalidateWord(), clean);
        }

        if (path != RunnerFile && path != CacheInvalidationsFile)
        {
            Add(findings, InvalidateText, TakePendingWord(), clean);
        }

        string forOtherCache = path == BusinessCalendarFile ? BusinessCalendarLazy().Replace(clean, m => new string(' ', m.Length)) : clean;
        Add(findings, OtherCacheText, OtherCache(), forOtherCache);
        if (path.EndsWith(".cshtml", StringComparison.Ordinal))
        {
            // A tag helper is markup; in C# the same letters are a generic argument.
            Add(findings, OtherCacheText, CacheTagHelper(), clean);
        }

        Add(findings, PageOutputText, PageOutput(), clean);

        findings.Sort((a, b) => a.Line != b.Line ? a.Line.CompareTo(b.Line) : string.CompareOrdinal(a.Rule, b.Rule));
        return findings;
    }

    /// <summary>Scans a dependency or deployment file line by line; a line that is only a comment is not scanned.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanDependencies(string text)
    {
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        List<(string Rule, int Line)> findings = [];
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            bool comment = trimmed.StartsWith('#') || trimmed.StartsWith("<!--", StringComparison.Ordinal) || trimmed.StartsWith("//", StringComparison.Ordinal);
            if (!comment && _dependency.IsMatch(lines[i]))
            {
                findings.Add((DependencyText, i + 1));
            }
        }

        return findings;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            AddAt(findings, rule, text, match.Index);
        }
    }

    private static void AddAt(List<(string Rule, int Line)> findings, string rule, string text, int index)
    {
        int line = text.AsSpan(0, index).Count('\n') + 1;
        if (!findings.Contains((rule, line)))
        {
            findings.Add((rule, line));
        }
    }

    private static string BlankCommentLines(string text)
    {
        StringBuilder builder = new();
        foreach (string line in text.Split('\n'))
        {
            string trimmed = line.TrimStart();
            bool comment = trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("@*", StringComparison.Ordinal);
            builder.Append(comment ? string.Empty : line).Append('\n');
        }

        return builder.ToString(0, builder.Length - 1);
    }

    [GeneratedRegex(@"\b(?:class|struct|interface|enum|record|delegate\s+[\w<>?,\s.]+?)\s+(?:class\s+|struct\s+)?(?<name>AppCache|CacheArea|CacheInvalidations)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Definition();

    [GeneratedRegex(@"\bInvalidate\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InvalidateWord();

    [GeneratedRegex(@"\bTakePendingAreas\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TakePendingWord();

    [GeneratedRegex(@"\bLazy\s*<\s*BusinessCalendar\s*>", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BusinessCalendarLazy();

    [GeneratedRegex(
        @"\w*MemoryCache\w*|\w*DistributedCache\w*|\w*CacheTagHelper\w*|\w*HybridCache\w*|\w*RedisCache\w*|\bObjectCache\b|\bSystem\s*\.\s*Runtime\s*\.\s*Caching\b|\bMicrosoft\s*\.\s*Extensions\s*\.\s*Caching\b|\bConcurrentDictionary\b|\bLazy\s*<|\bLazyInitializer\b|\bAsyncLazy\b",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex OtherCache();

    [GeneratedRegex(@"<\s*(?:distributed-)?cache\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CacheTagHelper();

    [GeneratedRegex(@"\w*OutputCach\w*|\w*ResponseCach\w*|\bCacheOutput\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PageOutput();
}
