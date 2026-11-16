using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The whole rule cell of a field row, read (T547 correction 5, F1; 04 7 introduction; 09 21.4 item 1). A rule cell is more than the ids <see cref="FormFieldCheck"/> knows: it holds clauses in plain words
/// ("ayrılmış adedin altına inemez"), references to the data rules (<c>02</c> section, <c>03</c> section), decisions (<c>K-</c>) and flows (<c>OB-</c>). Each of them is a kind of check the code must make:
/// <list type="bullet">
/// <item>the cell is split into clauses (<see cref="Split"/>) and every clause must be a key of <see cref="Kinds"/> - a rule text the table does not hold is <c>RuleClauseUnknown</c>, so nothing in a cell passes
/// unread, and a clause the table holds but no cell has is <c>RuleClauseNotInDocument</c>;</item>
/// <item>the kind of a clause (a <c>C:</c> kind) and every reference (<see cref="Atoms"/>) of the cell are demanded of the row like the kinds of its ids: the row has a line of that kind or
/// <c>RuleWithoutEnforcement</c> is reported (<see cref="FormFieldEnforcement"/>), and a line of such a kind the row's cell does not name is <c>RuleMissesAttribute</c>.</item>
/// </list>
/// A kind of this table needs no token (the line is one piece of code held exactly once by its file, the row's own); the kinds are the atoms of the cell, so a changed reference, a removed clause or a new clause
/// is reported whichever way the document is edited.
/// </summary>
internal static partial class FormFieldRuleClauses
{
    /// <summary>The clauses of a cell: split at <c>;</c> and at the dash (<c> — </c>) outside the parentheses, the back ticks removed.</summary>
    internal static IReadOnlyList<string> Split(string cell)
    {
        ArgumentNullException.ThrowIfNull(cell);

        string text = cell.Replace("`", string.Empty, StringComparison.Ordinal);
        List<string> parts = [];
        System.Text.StringBuilder current = new();
        int depth = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                depth--;
            }

            if (depth == 0 && c == ';')
            {
                parts.Add(current.ToString().Trim());
                current.Clear();
            }
            else if (depth == 0 && string.CompareOrdinal(text, i, " — ", 0, 3) == 0)
            {
                parts.Add(current.ToString().Trim());
                current.Clear();
                i += 2;
            }
            else
            {
                current.Append(c);
            }
        }

        parts.Add(current.ToString().Trim());
        return [.. parts.Where(p => p.Length > 0)];
    }

    /// <summary>
    /// The references of a cell as ids: <c>02§3.6.2</c> (a section carries the document of the last one named before it, so <c>, §3.6.7</c> after <c>02 §3.6.2</c> is <c>02§3.6.7</c>), <c>K-794</c> and <c>OB-15</c>.
    /// <c>K-1013</c> is not one: the row's rule ids carry it (<see cref="FormFieldEnforcement.RuleKinds"/>).
    /// </summary>
    internal static IReadOnlyList<string> Atoms(string cell)
    {
        ArgumentNullException.ThrowIfNull(cell);

        List<string> atoms = [];
        string prefix = string.Empty;
        foreach (Match match in Reference().Matches(cell.Replace("`", string.Empty, StringComparison.Ordinal)))
        {
            if (match.Groups[2].Success)
            {
                if (match.Groups[1].Success)
                {
                    prefix = match.Groups[1].Value;
                }

                atoms.Add(prefix + "§" + match.Groups[2].Value);
            }
            else
            {
                atoms.Add(match.Value);
            }
        }

        return [.. atoms.Distinct(StringComparer.Ordinal)];
    }

    /// <summary>
    /// What a rule cell demands of the row besides its ids: the problems of the clauses the table does not hold, and the kinds (the clauses' kinds and the references) the row's lines must make.
    /// </summary>
    internal static IReadOnlyList<string> Read(string row, string cell, out IReadOnlyList<string> demanded, ISet<string> usedClauses)
    {
        List<string> problems = [];
        List<string> kinds = [.. Atoms(cell)];
        foreach (string clause in Split(cell))
        {
            if (Kinds.TryGetValue(clause, out string[]? held))
            {
                usedClauses.Add(clause);
                kinds.AddRange(held);
            }
            else
            {
                problems.Add($"RuleClauseUnknown:{row}: the rule cell has the clause '{clause}', which the clause table does not hold (a new or changed rule text has no kind of check).");
            }
        }

        demanded = [.. kinds.Distinct(StringComparer.Ordinal)];
        return problems;
    }

    /// <summary>The clauses of the table that no rule cell of the document has.</summary>
    internal static IEnumerable<string> Stale(IReadOnlySet<string> usedClauses)
        => Kinds.Keys.Where(k => !usedClauses.Contains(k)).Select(k => $"RuleClauseNotInDocument:'{k}': the clause table holds a rule text that no rule cell of the document has.");

    /// <summary>Whether the kind is one of this table: a clause kind (<c>C:</c>) or a reference id.</summary>
    internal static bool IsKind(string kind)
        => kind.StartsWith("C:", StringComparison.Ordinal) || kind.Contains('§', StringComparison.Ordinal) || Reference().IsMatch(kind);

    [GeneratedRegex(@"(?:(0[23]) )?§(\d+(?:\.\d+)*)|\bK-(?!1013\b)\d+\b|\bOB-\d+\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Reference();
}
