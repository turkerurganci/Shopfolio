using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>One call of the code that names a field and the error code of a check on it in one expression: the file (relative to <c>src/Shopfolio.Web</c>), the field's name as the call writes it and the code.</summary>
internal sealed record FormFieldCodeCall(string File, string Field, string Code);

/// <summary>
/// The scan of the code for the checks that name a field and its error code together (D-1656; T547 criterion 6, code to row; 09 21.4 item 1, 19.2 item 3): every <c>FieldCodes.X</c> or <c>ErrorCode.X</c>
/// of a kind of check (a code that <see cref="FormFieldEnforcement.KindTokens"/> holds) with the field the same expression names - <c>nameof(Type.Member)</c>, a <c>...Fields.Member</c> constant or a
/// <c>...FieldName</c> local - nearest to it before the code, else after it, inside the same statement. A check that no line of <see cref="FormFieldEnforcementTable"/> (or of its place table) holds with both
/// is <c>CodeCheckNotInTable</c>: a check added to a handler, a module or a view does not enter the code without the table (and so without the row) noticing. The table-to-code direction is the line
/// held exactly once in its file (<c>EnforcementNotInCode</c>). The scan reads text only.
/// </summary>
internal static partial class FormFieldCodeCalls
{
    private const int Window = 120;

    private static readonly string[] SkippedFolders = ["Migrations"];

    /// <summary>The codes of the kinds of check: the names among the tokens of <see cref="FormFieldEnforcement.KindTokens"/> that are error codes (an identifier written with a capital letter and nothing else).</summary>
    public static IReadOnlySet<string> Codes { get; } = FormFieldEnforcement.KindTokens.Values.SelectMany(t => t).Where(t => CodeName().IsMatch(t)).ToHashSet(StringComparer.Ordinal);

    public static IReadOnlyList<FormFieldCodeCall> Scan(IReadOnlyDictionary<string, string> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        List<FormFieldCodeCall> calls = [];
        foreach ((string file, string text) in sources.Where(s => s.Key.EndsWith(".cs", StringComparison.Ordinal)).OrderBy(s => s.Key, StringComparer.Ordinal))
        {
            if (file.Split('/').Any(part => SkippedFolders.Contains(part, StringComparer.Ordinal)))
            {
                continue;
            }

            string visible = LineComment().Replace(text, string.Empty);
            foreach (Match code in CodeUse().Matches(visible))
            {
                string name = code.Groups["code"].Value;
                if (!Codes.Contains(name))
                {
                    continue;
                }

                string? field = NearestField(visible, code.Index, code.Index + code.Length);
                if (field is not null)
                {
                    calls.Add(new FormFieldCodeCall(file, field, name));
                }
            }
        }

        return calls;
    }

    /// <summary>
    /// The calls of the code that no line of the table holds with both the field and the code, as <c>file: Field, Code</c> lines, and the exemptions that are not (or no longer) such a call. A line holds a call when
    /// its fragment, white space read as one space, names the field as a whole word and carries <c>.Code</c> - and is a line of the same file.
    /// </summary>
    public static IReadOnlyList<string> Problems(IReadOnlyDictionary<string, string> sources, IReadOnlyDictionary<string, string[]> lines, IReadOnlyList<FormFieldCodeCall> unlisted)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(unlisted);

        List<(string File, string Fragment)> fragments = [];
        foreach (string line in lines.Values.SelectMany(l => l))
        {
            int bar = line.IndexOf('|', StringComparison.Ordinal);
            int hash = line.IndexOf('#', StringComparison.Ordinal);
            if (bar > 0 && hash > bar)
            {
                fragments.Add((line[(bar + 1)..hash], FormFieldCheck.Collapse(line[(hash + 1)..])));
            }
        }

        IReadOnlyList<FormFieldCodeCall> scanned = Scan(sources);
        HashSet<FormFieldCodeCall> unclaimed = [.. scanned.Where(c => !IsHeld(c, fragments))];
        List<string> problems = [];
        foreach (FormFieldCodeCall call in unclaimed.Except(unlisted).OrderBy(c => c.File, StringComparer.Ordinal).ThenBy(c => c.Field, StringComparer.Ordinal).ThenBy(c => c.Code, StringComparer.Ordinal))
        {
            problems.Add($"CodeCheckNotInTable: {call.File}: {call.Field} is checked with {call.Code} and no line of the enforcement tables holds the two together.");
        }

        foreach (FormFieldCodeCall exempt in unlisted)
        {
            if (!scanned.Contains(exempt))
            {
                problems.Add($"DeadExemption: {exempt.File}: {exempt.Field}, {exempt.Code} is not in the code.");
            }
            else if (!unclaimed.Contains(exempt))
            {
                problems.Add($"ExemptAndHeld: {exempt.File}: {exempt.Field}, {exempt.Code} is held by a line of the table and is listed as an exemption too.");
            }
        }

        return problems;
    }

    private static bool IsHeld(FormFieldCodeCall call, List<(string File, string Fragment)> fragments)
        => fragments.Any(f => string.Equals(f.File, call.File, StringComparison.Ordinal)
            && f.Fragment.Contains("." + call.Code, StringComparison.Ordinal)
            && WholeWord(f.Fragment, call.Field));

    private static bool WholeWord(string text, string word)
    {
        for (int at = text.IndexOf(word, StringComparison.Ordinal); at >= 0; at = text.IndexOf(word, at + word.Length, StringComparison.Ordinal))
        {
            bool before = at == 0 || !IsIdentifierPart(text[at - 1]);
            bool after = at + word.Length >= text.Length || !IsIdentifierPart(text[at + word.Length]);
            if (before && after)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsIdentifierPart(char c) => char.IsLetterOrDigit(c) || c == '_';

    /// <summary>The field the expression of a code names: the last field before the code inside its statement window, else the first after it.</summary>
    private static string? NearestField(string text, int codeStart, int codeEnd)
    {
        int from = Math.Max(0, codeStart - Window);
        string before = text[from..codeStart];
        int cut = before.LastIndexOfAny([';', '{', '}']);
        if (cut >= 0)
        {
            before = before[(cut + 1)..];
        }

        Match? last = FieldExpression().Matches(before).LastOrDefault();
        if (last is not null)
        {
            return last.Groups["field"].Value;
        }

        string after = text[codeEnd..Math.Min(text.Length, codeEnd + Window)];
        cut = after.IndexOfAny([';', '{', '}']);
        if (cut >= 0)
        {
            after = after[..cut];
        }

        Match first = FieldExpression().Match(after);
        return first.Success ? first.Groups["field"].Value : null;
    }

    [GeneratedRegex(@"\b(?:FieldCodes|ErrorCode)\.(?<code>[A-Z]\w+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeUse();

    [GeneratedRegex(@"nameof\((?:[\w]+\.)*(?<field>\w+)\)|\b\w+Fields\.(?<field>\w+)|\b(?<field>\w*FieldName)\b|(?:FieldError|FieldRejection|Rejection|Problem)\(\s*\$?""(?<field>[^""\r\n]+)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FieldExpression();

    [GeneratedRegex(@"^[A-Z]\w+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeName();

    [GeneratedRegex(@"//[^\n]*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();
}
