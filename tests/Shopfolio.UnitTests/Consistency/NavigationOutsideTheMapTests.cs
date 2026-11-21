using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The two writings of an exit that the navigation scan (<c>NavigationSites</c>) does not read (T604; D-1780): an <c>&lt;a href&gt;</c> written inside a C# string (the error pages of the request layers,
/// <c>04 2.7.3.2</c>) and a script's <c>location.assign</c> (the cancel link, the return from the payment page). The scan reads <c>.cshtml</c> links, form targets, handler redirects and empty-state exits only;
/// <c>04 3</c> says so (K-1632). The two writings are pinned here by file and count: a new one is a red test that names the file, and the answer is to add it to the list with its reason - or to write
/// the exit where the scan reads it - not to leave it outside the map unseen.
/// </summary>
[Trait("Consistency", "true")]
public sealed class NavigationOutsideTheMapTests
{
    private static readonly string[] SkippedFolders = ["bin", "obj", "Migrations"];

    /// <summary>A link written in a C# string, whatever the quoting of the string (escaped, verbatim or raw): the tag and its attribute.</summary>
    private static readonly Regex StringLink = new(@"<a\s+href=", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    /// <summary>A script that moves the window: <c>location.assign</c>, <c>location.replace</c> or an assignment to <c>location.href</c>.</summary>
    private static readonly Regex ScriptExit = new(@"location\.(assign|replace)\s*\(|location\.href\s*=[^=]", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    /// <summary>Files that write the exit of an error page as a C# string, with the count of such links in each (04 2.7.3.2: "Yeniden dene" and "Ana sayfaya dön").</summary>
    private static readonly IReadOnlyDictionary<string, int> KnownStringLinks = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["Infrastructure/Errors/UnexpectedErrorLayer.cs"] = 2,
        ["Infrastructure/Security/RequestBodyLimits.cs"] = 1,
        ["Infrastructure/Security/RequestRateLimiting.cs"] = 1,
    };

    /// <summary>Scripts that move the window by an address, with the count (the cancel link of the form pages; the return from the payment page to the order).</summary>
    private static readonly IReadOnlyDictionary<string, int> KnownScriptExits = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["wwwroot/js/common.js"] = 1,
        ["wwwroot/js/pages/payment/return.js"] = 1,
    };

    private static string Web => Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web");

    /// <summary>Counts the pattern per file, by the file's name as given; a file with no match is left out.</summary>
    private static SortedDictionary<string, int> CountPerFile(IReadOnlyDictionary<string, string> files, Regex pattern)
    {
        SortedDictionary<string, int> counts = new(StringComparer.Ordinal);
        foreach ((string path, string text) in files)
        {
            int count = pattern.Count(text);
            if (count > 0)
            {
                counts[path] = count;
            }
        }

        return counts;
    }

    /// <summary>The files of one kind under the web project (or one folder of it), by path relative to the web project.</summary>
    private static Dictionary<string, string> Read(string folder, string extension)
    {
        Dictionary<string, string> files = new(StringComparer.Ordinal);
        foreach (string file in Directory.EnumerateFiles(Path.Combine(Web, folder), "*" + extension, SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(Web, file).Replace('\\', '/');
            if (!relative.Split('/').Any(part => SkippedFolders.Contains(part, StringComparer.Ordinal)))
            {
                files[relative] = File.ReadAllText(file);
            }
        }

        return files;
    }

    [Fact]
    public void TheLinksWrittenInCSharpStrings_AreExactlyTheKnownErrorPageExits()
    {
        SortedDictionary<string, int> found = CountPerFile(Read(".", ".cs"), StringLink);

        Assert.Equal(KnownStringLinks.OrderBy(k => k.Key, StringComparer.Ordinal), found.AsEnumerable());
    }

    [Fact]
    public void TheScriptsThatMoveTheWindow_AreExactlyTheKnownTwo()
    {
        SortedDictionary<string, int> found = CountPerFile(Read("wwwroot/js", ".js"), ScriptExit);

        Assert.Equal(KnownScriptExits.OrderBy(k => k.Key, StringComparer.Ordinal), found.AsEnumerable());
    }

    [Fact]
    public void ANewStringLinkOrScriptExit_IsFoundByThePatterns_WhateverItsQuoting()
    {
        Dictionary<string, string> sources = new(StringComparer.Ordinal)
        {
            ["A.cs"] = "var a = \"<a href=\\\"/yeni\\\">Git</a>\";",
            ["B.cs"] = "var b = \"\"\"<a href=\"/yeni\">Git</a>\"\"\";",
            ["C.cs"] = "var c = @\"<a href=\"\"/yeni\"\">Git</a>\";",
            ["D.cs"] = "var d = 1;",
        };
        Dictionary<string, string> scripts = new(StringComparer.Ordinal)
        {
            ["a.js"] = "window.location.assign(next);",
            ["b.js"] = "window.top.location.replace(next);",
            ["c.js"] = "location.href = next;",
            ["d.js"] = "if (location.href == other) { }",
        };

        Assert.Equal(["A.cs", "B.cs", "C.cs"], CountPerFile(sources, StringLink).Keys);
        Assert.Equal(["a.js", "b.js", "c.js"], CountPerFile(scripts, ScriptExit).Keys);
    }

    [Fact]
    public void TheNavigationMap_SaysWhichWritingsTheScanDoesNotRead()
    {
        string map = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "04_UI_SPECS.md"));
        int start = map.IndexOf("## 3. Navigasyon haritası", StringComparison.Ordinal);
        int end = map.IndexOf("### 3.1 ", start, StringComparison.Ordinal);

        Assert.True(start >= 0 && end > start);
        string intro = map[start..end];
        Assert.Contains("Taramanın kapsamı", intro, StringComparison.Ordinal);
        Assert.Contains("location.assign", intro, StringComparison.Ordinal);
        Assert.Contains("C# dizgesiyle", intro, StringComparison.Ordinal);
    }
}
