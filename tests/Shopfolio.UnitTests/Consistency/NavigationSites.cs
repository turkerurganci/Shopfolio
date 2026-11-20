using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The ways the code moves a user to another place: a link (<c>&lt;a href&gt;</c> of a view), a redirect (a handler's 303 or a session handler's redirect) and the way out of an empty list - the third
/// argument of the call that gives a shared empty-state component its exit (<c>EmptyStates.WithExit</c>, <c>WithFirstRecordButton</c>, <c>new EmptyState</c>). The component's own link is no site of a
/// screen: it draws what each screen gave it, so each call is the site (T546 correction 3).
/// </summary>
internal enum NavigationSiteKind
{
    Link,
    Redirect,
    EmptyExit,

    /// <summary>The target of a form: the <c>action</c> of a <c>&lt;form&gt;</c>, a button's <c>formaction</c>, an <c>asp-page</c> of a form element (D-1626). A form that posts to a handler of its own screen is no site.</summary>
    FormAction,
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
    private const string HandlerMarker = "?handler=";

    private const string RedirectDefinitions ="Infrastructure/Forms/FormResults.cs";

    /// <summary>The constructors of the empty-state view are the definition of the way out, not a screen's own call.</summary>
    private const string EmptyExitDefinitions = "Pages/Components/StateModels.cs";

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
                sites.AddRange(FormActions(file, text));
            }
            else if (!string.Equals(file, RedirectDefinitions, StringComparison.Ordinal))
            {
                sites.AddRange(Redirects(file, text));
            }

            if (!string.Equals(file, EmptyExitDefinitions, StringComparison.Ordinal))
            {
                sites.AddRange(EmptyExits(file, text));
            }
        }

        return sites;
    }

    /// <summary>The way out of an empty list: the third argument of each call that builds an empty state (the sentence, the label, then the address). A call without an exit has no site.</summary>
    private static IEnumerable<NavigationSite> EmptyExits(string file, string text)
    {
        string visible = file.EndsWith(".cshtml", StringComparison.Ordinal) ? RazorComment().Replace(text, string.Empty) : text;
        foreach (Match call in EmptyStateCall().Matches(visible))
        {
            int lineStart = visible.LastIndexOf('\n', call.Index) + 1;
            if (visible[lineStart..call.Index].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> arguments = [];
            int at = call.Index + call.Length;
            while (at < visible.Length)
            {
                string argument = Value(visible, at, ')', ',');
                arguments.Add(argument);
                at += argument.Length;
                if (at >= visible.Length || visible[at] == ')')
                {
                    break;
                }

                at++;
            }

            if (arguments.Count >= 3)
            {
                yield return new NavigationSite(file, NavigationSiteKind.EmptyExit, Collapse(StripLineComments(arguments[2])));
            }
        }
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

    /// <summary>
    /// The targets of the forms of a view (D-1626): the <c>action</c> of a form, the <c>formaction</c> of a button and the <c>asp-page</c> of a form element, as the expression of the address. A form that posts
    /// to a handler of its own screen (<c>?handler=</c> after an address that is no literal path, or after a local whose definition in the same file carries it) stays on the screen and is no site; a
    /// literal path before <c>?handler=</c> is another screen's handler and stays a site.
    /// </summary>
    private static IEnumerable<NavigationSite> FormActions(string file, string text)
    {
        string visible = RazorComment().Replace(text, string.Empty);
        foreach (Match start in FormTarget().Matches(visible))
        {
            string value = Collapse(Value(visible, start.Index + start.Length, '"'));
            if (value.Length == 0 || value.StartsWith('#') || PostsToOwnHandler(visible, value))
            {
                continue;
            }

            yield return new NavigationSite(file, NavigationSiteKind.FormAction, value);
        }
    }

    private static bool PostsToOwnHandler(string visible, string value)
    {
        if (value.Contains(HandlerMarker, StringComparison.Ordinal))
        {
            return !value.StartsWith('/');
        }

        Match local = LeadingLocal().Match(value);
        if (!local.Success)
        {
            return false;
        }

        string definition = "string " + local.Groups["name"].Value + " =";
        int at = visible.IndexOf(definition, StringComparison.Ordinal);
        if (at < 0)
        {
            return false;
        }

        int end = visible.IndexOf(';', at);
        string defined = end < 0 ? visible[at..] : visible[at..end];
        return defined.Contains(HandlerMarker, StringComparison.Ordinal) && !defined.Contains("= \"/", StringComparison.Ordinal);
    }

    /// <summary>
    /// The screens that name where an uncertain outcome goes: each method <c>string OutcomeScreen() =&gt; ...;</c> of the web project, as the file and the body, whitespace collapsed (D-1636). The interface's
    /// own declaration has no body and is none of them.
    /// </summary>
    public static IReadOnlyList<(string File, string Body)> OutcomeScreens(IReadOnlyDictionary<string, string> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        List<(string, string)> screens = [];
        foreach ((string file, string text) in sources.Where(s => s.Key.EndsWith(".cs", StringComparison.Ordinal)).OrderBy(s => s.Key, StringComparer.Ordinal))
        {
            foreach (Match definition in OutcomeScreenDefinition().Matches(text))
            {
                int lineStart = text.LastIndexOf('\n', definition.Index) + 1;
                if (text[lineStart..definition.Index].TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                screens.Add((file, Collapse(StripLineComments(Value(text, definition.Index + definition.Length, ';')))));
            }
        }

        return screens;
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
            int at = call.Index + call.Length;
            if (call.Groups["name"].Value == "WriteSeeOther")
            {
                // The one written straight on a response takes the response first, then the address (FormResults.WriteSeeOther; D-1626).
                at += Value(text, at, ')', ',').Length + 1;
            }

            string arguments = Value(text, at, ')', wholeList ? null : ',');
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

    [GeneratedRegex(@"<form\b(?:[^>""]|""[^""]*"")*?(?<![\w-])action=""|(?<![\w-])formaction=""|(?<![\w-])asp-page=""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FormTarget();

    [GeneratedRegex(@"^@\(?(?<name>[A-Za-z_]\w*)\)?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LeadingLocal();

    [GeneratedRegex(@"\bstring\s+OutcomeScreen\(\)\s*=>", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OutcomeScreenDefinition();

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorComment();

    [GeneratedRegex(@"(?<![\w.])(?:FormResults\.(?<name>SeeOtherToGoogle|SeeOther|WriteSeeOther)|(?<name>Redirect)|(?<name>RedirectToPage|LocalRedirect|RedirectPermanent|RedirectToRoute|RedirectToAction))\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RedirectCall();

    [GeneratedRegex(@"(?<![\w.])(?:EmptyStates\.(?:WithExit|WithFirstRecordButton)|new EmptyState(?:View)?)\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmptyStateCall();

    [GeneratedRegex(@"//[^\n]*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Whitespace();
}
