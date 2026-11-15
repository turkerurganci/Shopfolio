using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>The two ways the code moves a user to another place: a link (<c>&lt;a href&gt;</c> of a view) and a redirect (a handler's 303 or a session handler's redirect).</summary>
internal enum NavigationSiteKind
{
    Link,
    Redirect,
}

/// <summary>One place of the code that leads away: the file (relative to <c>src/Shopfolio.Web</c>), the kind and the expression of the address, whitespace collapsed.</summary>
internal sealed record NavigationSite(string File, NavigationSiteKind Kind, string Key);

/// <summary>
/// The scan of the code for navigation (T546; 04 3; 09 21.4 item 1, 19.2 item 3): every link of a Razor view and every redirect call of the web project, as the expression of the address. A link
/// whose address starts with <c>#</c> stays on the page and is no site. The scan reads text only; the checks that must fail on a changed file read a changed copy of the text, never the file.
/// </summary>
internal static partial class NavigationSites
{
    private static readonly string[] SkippedFolders = ["bin", "obj", "Migrations"];

    /// <summary>The definitions of the redirect helpers are not call sites.</summary>
    private const string RedirectDefinitions = "Infrastructure/Forms/FormResults.cs";

    /// <summary>The source text of the web project: <c>.cs</c> and <c>.cshtml</c> files by path relative to <c>src/Shopfolio.Web</c>.</summary>
    public static IReadOnlyDictionary<string, string> Load(string root)
    {
        string web = Path.Combine(root, "src", "Shopfolio.Web");
        Dictionary<string, string> sources = new(StringComparer.Ordinal);
        foreach (string file in Directory.EnumerateFiles(web, "*.*", SearchOption.AllDirectories).Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".cshtml", StringComparison.Ordinal)))
        {
            string relative = Path.GetRelativePath(web, file).Replace('\\', '/');
            if (relative.Split('/').Any(part => SkippedFolders.Contains(part, StringComparer.Ordinal)))
            {
                continue;
            }

            sources[relative] = File.ReadAllText(file).ReplaceLineEndings("\n");
        }

        return sources;
    }

    public static IReadOnlyList<NavigationSite> Scan(IReadOnlyDictionary<string, string> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        List<NavigationSite> sites = [];
        foreach ((string file, string text) in sources.OrderBy(s => s.Key, StringComparer.Ordinal))
        {
            if (file.EndsWith(".cshtml", StringComparison.Ordinal))
            {
                sites.AddRange(Links(file, text));
            }
            else if (!string.Equals(file, RedirectDefinitions, StringComparison.Ordinal))
            {
                sites.AddRange(Redirects(file, text));
            }
        }

        return sites;
    }

    private static IEnumerable<NavigationSite> Links(string file, string text)
    {
        string visible = RazorComment().Replace(text, string.Empty);
        foreach (Match start in AnchorHref().Matches(visible))
        {
            string value = Value(visible, start.Index + start.Length, '"');
            if (!value.StartsWith('#'))
            {
                yield return new NavigationSite(file, NavigationSiteKind.Link, Collapse(value));
            }
        }
    }

    private static IEnumerable<NavigationSite> Redirects(string file, string text)
    {
        foreach (Match call in RedirectCall().Matches(text))
        {
            int lineStart = text.LastIndexOf('\n', call.Index) + 1;
            string before = text[lineStart..call.Index];
            if (before.TrimStart().StartsWith("//", StringComparison.Ordinal) || before.TrimEnd().EndsWith("void", StringComparison.Ordinal))
            {
                continue;
            }

            // The helpers take the address first; the session handler's own Redirect takes the status and then the address, and its whole list is the key.
            bool wholeList = call.Groups["name"].Value == "Redirect";
            string arguments = Value(text, call.Index + call.Length, ')', wholeList ? null : ',');
            yield return new NavigationSite(file, NavigationSiteKind.Redirect, Collapse(StripLineComments(arguments)));
        }
    }

    /// <summary>The text from <paramref name="start"/> to the first <paramref name="end"/> (or <paramref name="alternative"/>) that stands outside parentheses and string literals.</summary>
    private static string Value(string text, int start, char end, char? alternative = null)
    {
        StringBuilder value = new();
        int depth = 0;
        bool inString = false;
        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];
            if (inString)
            {
                inString = c != '"' || text[i - 1] == '\\';
            }
            else if (c == '"' && (depth > 0 || end != '"'))
            {
                inString = true;
            }
            else if (c == '(')
            {
                depth++;
            }
            else if (c == ')' && depth > 0)
            {
                depth--;
            }
            else if (depth == 0 && (c == end || c == alternative))
            {
                break;
            }

            value.Append(c);
        }

        return value.ToString();
    }

    private static string StripLineComments(string text) => LineComment().Replace(text, string.Empty);

    private static string Collapse(string text) => Whitespace().Replace(text, " ").Trim();

    [GeneratedRegex(@"<a\b[^>]*?\bhref=""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AnchorHref();

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorComment();

    [GeneratedRegex(@"(?<![\w.])(?:FormResults\.(?<name>SeeOtherToGoogle|SeeOther)|(?<name>Redirect)|(?<name>RedirectToPage|LocalRedirect|RedirectPermanent|RedirectToRoute|RedirectToAction))\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RedirectCall();

    [GeneratedRegex(@"//[^\n]*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Whitespace();
}
