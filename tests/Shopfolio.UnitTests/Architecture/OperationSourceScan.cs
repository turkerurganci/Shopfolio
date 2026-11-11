using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the available-operations pattern (T217 criteria 2, 3, 4, 8, KK2, KK4, KK8; 07 8.1.1, 8.1.2, 8.1.5; 09 8.3). Two parts, both on the text with the comments and the
/// text of string literals blanked, the whole file read (a call split over lines is found):
/// <list type="number">
/// <item>A POST handler marked <c>[GatedOperation]</c> (any spelling: <c>GatedOperation</c>, <c>GatedOperationAttribute</c>, a qualified name, among other attributes in one list or in a list of its own, with
/// arguments) must reach the gate: its body, or the body of a method of the same file that the body calls (followed to a depth of four), calls <c>OperationGate.Decide</c> or runs a
/// <c>OperationRunner</c>. A mark on a method that is not an <c>OnPost</c> handler is refused too - a GET does not apply an operation.</item>
/// <item>A view or a script decides no operation: a view names no status type, no status member of the closed lists the caller hands in, and does no date arithmetic
/// (<c>TurkeyTime</c>, <c>AddDays</c>, <c>DateTime</c>, <c>TimeProvider</c>); a script names no status member and reads no clock (<c>Date</c>, <c>Date.now</c>, <c>getTime</c>). The values
/// the page needs come from the server's availability (07 8.1.4).</item>
/// </list>
/// There is no allowed file. The test projects are not scanned for part 2 (their samples are read by their own test); the sample page is scanned for part 1.
/// </summary>
internal static partial class OperationSourceScan
{
    public const string GatedWithoutCheck = "has a [GatedOperation] handler that never reaches OperationGate.Decide or OperationRunner (07 8.1.2: the POST re-checks the rule)";
    public const string GatedNotAPostHandler = "marks a method that is not an OnPost handler with [GatedOperation] (07 8.1.2)";
    public const string ViewDecides = "a view names a status or does date arithmetic (07 8.1.1: the browser and the view derive no operation; the server's availability decides)";
    public const string ScriptDecides = "a script names a status or reads the clock (07 8.1.1, 8.1.4: the script decides no operation and derives no last day)";

    private const int FollowDepth = 4;

    public static IReadOnlyList<(string Rule, int Line)> ScanHandlers(string text)
    {
        string clean = Blank(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        foreach (Match mark in GatedMark().Matches(clean))
        {
            int line = clean.AsSpan(0, mark.Index).Count('\n') + 1;
            int afterList = SkipAttributeLists(clean, mark.Index + mark.Length);
            Match head = MethodHead().Match(clean, afterList);
            if (!head.Success || head.Index != afterList)
            {
                findings.Add((GatedNotAPostHandler, line));
                continue;
            }

            string name = head.Groups["name"].Value;
            if (!name.StartsWith("OnPost", StringComparison.Ordinal))
            {
                findings.Add((GatedNotAPostHandler, line));
                continue;
            }

            string? body = BodyAt(clean, head.Index + head.Length);
            if (body is null || !Reaches(clean, body, FollowDepth, []))
            {
                findings.Add((GatedWithoutCheck, line));
            }
        }

        return [.. findings.Distinct().OrderBy(f => f.Line)];
    }

    public static IReadOnlyList<(string Rule, int Line)> ScanClient(string relativePath, string text, IReadOnlyCollection<string> statusTypes, IReadOnlyCollection<string> statusMembers)
    {
        string path = relativePath.Replace('\\', '/');
        bool view = path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
        bool script = path.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
        if (!view && !script)
        {
            return [];
        }

        // A view's Razor comment and a script's comments are blanked; the words are searched in the code and the markup, strings included
        // (a status word in an attribute value or a string is exactly what a script would compare), so only the comments are blanked here.
        string clean = view ? BlankViewComments(text.ReplaceLineEndings("\n")) : BlankScriptComments(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        if (view)
        {
            Add(findings, ViewDecides, WordsRegex([.. statusTypes, "TurkeyTime", "AddDays", "AddMonths", "DateTime", "DateTimeOffset", "TimeProvider", "LastDayOfPeriod", "DateOnly.FromDateTime"]), clean);
        }
        else
        {
            Add(findings, ScriptDecides, WordsRegex([.. statusMembers, "Date", "getTime", "getFullYear", "getDate"]), clean);
        }

        return [.. findings.Distinct().OrderBy(f => f.Line)];
    }

    private static Regex WordsRegex(IEnumerable<string> words) =>
        new(@"(?<![A-Za-z0-9_])(?:" + string.Join('|', words.Distinct(StringComparer.Ordinal).Select(Regex.Escape)) + @")(?![A-Za-z0-9_])", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            findings.Add((rule, text.AsSpan(0, match.Index).Count('\n') + 1));
        }
    }

    // Whether the body reaches the gate itself or through a method of the same file it calls.
    private static bool Reaches(string file, string body, int depth, HashSet<string> seen)
    {
        if (Gate().IsMatch(body))
        {
            return true;
        }

        if (depth == 0)
        {
            return false;
        }

        foreach (Match call in Call().Matches(body))
        {
            string name = call.Groups["name"].Value;
            if (!seen.Add(name))
            {
                continue;
            }

            foreach (Match definition in MethodHeadAnywhere().Matches(file))
            {
                if (definition.Groups["name"].Value != name)
                {
                    continue;
                }

                string? inner = BodyAt(file, definition.Index + definition.Length);
                if (inner is not null && Reaches(file, inner, depth - 1, seen))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // The body of the block that starts at or after the position (the head ended before it); an expression body is read to its semicolon.
    private static string? BodyAt(string text, int position)
    {
        int i = position;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
        {
            i++;
        }

        if (i >= text.Length)
        {
            return null;
        }

        if (text[i] == '{')
        {
            int depth = 0;
            for (int j = i; j < text.Length; j++)
            {
                if (text[j] == '{')
                {
                    depth++;
                }
                else if (text[j] == '}' && --depth == 0)
                {
                    return text[(i + 1)..j];
                }
            }

            return null;
        }

        if (text.AsSpan(i).StartsWith("=>", StringComparison.Ordinal))
        {
            int end = text.IndexOf(';', i);
            return end < 0 ? null : text[(i + 2)..end];
        }

        return null;
    }

    // After the attribute list that holds the mark, any further attribute lists.
    private static int SkipAttributeLists(string text, int position)
    {
        int i = position;
        // The rest of the list that holds the mark: up to its closing bracket, brackets of the arguments balanced.
        int depth = 1;
        while (i < text.Length && depth > 0)
        {
            if (text[i] == '[')
            {
                depth++;
            }
            else if (text[i] == ']')
            {
                depth--;
            }

            i++;
        }

        while (true)
        {
            int j = i;
            while (j < text.Length && char.IsWhiteSpace(text[j]))
            {
                j++;
            }

            if (j < text.Length && text[j] == '[')
            {
                int d = 0;
                for (; j < text.Length; j++)
                {
                    if (text[j] == '[')
                    {
                        d++;
                    }
                    else if (text[j] == ']' && --d == 0)
                    {
                        j++;
                        break;
                    }
                }

                i = j;
                continue;
            }

            return j;
        }
    }

    // Comments and the text of string and character literals become spaces of the same line count, so a name in a comment or a string reaches nothing.
    private static string Blank(string text)
    {
        StringBuilder result = new(text.Length);
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            char next = i + 1 < text.Length ? text[i + 1] : '\0';
            if (c == '/' && next == '/')
            {
                while (i < text.Length && text[i] != '\n')
                {
                    result.Append(' ');
                    i++;
                }
            }
            else if (c == '/' && next == '*')
            {
                result.Append("  ");
                i += 2;
                while (i < text.Length && !(text[i] == '*' && i + 1 < text.Length && text[i + 1] == '/'))
                {
                    result.Append(text[i] == '\n' ? '\n' : ' ');
                    i++;
                }

                result.Append("  ");
                i = Math.Min(text.Length, i + 2);
            }
            else if (c == '"' || (c is '$' or '@' && (next == '"' || (next is '$' or '@' && i + 2 < text.Length && text[i + 2] == '"'))))
            {
                bool verbatim = false;
                int start = i;
                while (text[i] != '"')
                {
                    verbatim |= text[i] == '@';
                    i++;
                }

                result.Append(' ', i - start);
                result.Append('"');
                i++;
                while (i < text.Length)
                {
                    if (text[i] == '\\' && !verbatim)
                    {
                        result.Append("  ");
                        i += 2;
                        continue;
                    }

                    if (text[i] == '"')
                    {
                        if (verbatim && i + 1 < text.Length && text[i + 1] == '"')
                        {
                            result.Append("  ");
                            i += 2;
                            continue;
                        }

                        break;
                    }

                    result.Append(text[i] == '\n' ? '\n' : ' ');
                    i++;
                }

                result.Append('"');
                i++;
            }
            else if (c == '\'' && i + 2 < text.Length && (text[i + 2] == '\'' || (text[i + 1] == '\\' && i + 3 < text.Length)))
            {
                int end = text[i + 1] == '\\' ? text.IndexOf('\'', i + 3) : i + 2;
                end = end < 0 ? text.Length - 1 : end;
                result.Append(' ', end - i + 1);
                i = end + 1;
            }
            else
            {
                result.Append(c);
                i++;
            }
        }

        return result.ToString();
    }

    private static string BlankViewComments(string text) =>
        ViewComment().Replace(text, m => string.Concat(m.Value.Select(c => c == '\n' ? '\n' : ' ')));

    private static string BlankScriptComments(string text) =>
        ScriptComment().Replace(text, m => string.Concat(m.Value.Select(c => c == '\n' ? '\n' : ' ')));

    [GeneratedRegex(@"@\*.*?\*@|<!--.*?-->", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ViewComment();

    [GeneratedRegex(@"/\*.*?\*/|//[^\n]*", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScriptComment();

    // [GatedOperation], [GatedOperationAttribute], [Operations.GatedOperation(...)], [HttpPost, GatedOperation]: the name as the end of an attribute in a list.
    [GeneratedRegex(@"(?<=[\[,]\s*)(?:[A-Za-z_][A-Za-z0-9_]*\s*\.\s*)*GatedOperation(?:Attribute)?\b(?:\s*\([^)]*\))?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex GatedMark();

    // The head of a method after its attributes: modifiers, return type, name, the parameter list and what comes before the body.
    [GeneratedRegex(@"(?:(?:public|private|protected|internal|static|async|virtual|override|sealed|new|partial)\s+)*[A-Za-z_][\w<>,\.\?\[\]\s]*?\s+(?<name>[A-Za-z_]\w*)\s*(?:<[^>(]*>)?\s*\((?:[^()]|\([^()]*\))*\)\s*(?:where[^{=]*)?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MethodHead();

    [GeneratedRegex(@"(?<![\w.])(?:(?:public|private|protected|internal|static|async|virtual|override|sealed|new|partial)\s+)+[A-Za-z_][\w<>,\.\?\[\]\s]*?\s+(?<name>[A-Za-z_]\w*)\s*(?:<[^>(]*>)?\s*\((?:[^()]|\([^()]*\))*\)\s*(?:where[^{=]*)?(?=\s*(?:\{|=>))", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MethodHeadAnywhere();

    [GeneratedRegex(@"(?<![\w])(?<name>[A-Za-z_]\w*)\s*(?:<[^>(]*>)?\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Call();

    // The gate: OperationGate.Decide(...) (also qualified), or a OperationRunner used (a field, a parameter or a resolved service of that type) and run - its
    // .RunAsync( called after it (09 8.3 item 2 (a); D-154 (a)): a handler that only resolves the runner never reaches the gate.
    [GeneratedRegex(@"\bOperationGate\s*\.\s*Decide\b|\bOperationRunner\b[\s\S]*?\.\s*RunAsync\s*\(",RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Gate();
}
