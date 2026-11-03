using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Reads the enum tables of the documents (09 21.4 item 1; T102 KK1): <c>06 5.2</c>…<c>5.6</c> write each enum under a paragraph
/// headed <c>**5.n.m `Name` …**</c> as a table whose second column is the code name; <c>09 9.2</c> item 3 writes the audit operation codes
/// in the second column of its table.
/// </summary>
internal static partial class EnumTable
{
    /// <summary>The code names of every enum of <c>06 5</c>, by enum name, in the order the document gives them.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> ReadDataModelEnums(string document)
    {
        Dictionary<string, IReadOnlyList<string>> enums = new(StringComparer.Ordinal);
        string[] lines = document.ReplaceLineEndings("\n").Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith("## 5. ", StringComparison.Ordinal));
        int end = Array.FindIndex(lines, start + 1, l => l.StartsWith("## 6. ", StringComparison.Ordinal));
        if (start < 0 || end < 0)
        {
            throw new InvalidOperationException("The enum chapter of the data model document was not found.");
        }

        string? current = null;
        List<string> names = [];
        for (int i = start; i < end; i++)
        {
            string line = lines[i];
            if (Heading().IsMatch(line))
            {
                Flush(enums, current, names);
                Match named = NamedHeading().Match(line);
                current = named.Success ? named.Groups[1].Value : null;
                names = [];
            }
            else if (current is not null && line.StartsWith('|'))
            {
                string[] cells = line.Split('|');
                Match code = cells.Length > 2 ? CodeCell().Match(cells[2]) : Match.Empty;
                if (code.Success)
                {
                    names.Add(code.Groups[1].Value);
                }
            }
        }

        Flush(enums, current, names);
        return enums;
    }

    /// <summary>The audit operation codes of <c>09 9.2</c> item 3, in the order the document gives them.</summary>
    public static IReadOnlyList<string> ReadAuditOperations(string document)
    {
        string[] lines = document.ReplaceLineEndings("\n").Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith("3. **Kod listesi", StringComparison.Ordinal));
        if (start < 0)
        {
            throw new InvalidOperationException("The audit operation code list of the coding guidelines was not found.");
        }

        List<string> codes = [];
        for (int i = start + 1; i < lines.Length && !lines[i].StartsWith("   Toplam:", StringComparison.Ordinal); i++)
        {
            string line = lines[i].TrimStart();
            if (!line.StartsWith('|') || line.StartsWith("|---", StringComparison.Ordinal) || line.StartsWith("| Alan", StringComparison.Ordinal))
            {
                continue;
            }

            string[] cells = line.Split('|');
            string withoutSubjects = SubjectNote().Replace(cells[2], string.Empty);
            codes.AddRange(BacktickToken().Matches(withoutSubjects).Select(m => m.Groups[1].Value));
        }

        return codes;
    }

    private static void Flush(Dictionary<string, IReadOnlyList<string>> enums, string? name, List<string> names)
    {
        if (name is not null)
        {
            enums[name] = names;
        }
    }

    [GeneratedRegex(@"^(\*\*5\.\d+\.\d+ |### )")]
    private static partial Regex Heading();

    [GeneratedRegex(@"^\*\*5\.\d+\.\d+ `(\w+)`")]
    private static partial Regex NamedHeading();

    [GeneratedRegex(@"^\s*`(\w+)`\s*$")]
    private static partial Regex CodeCell();

    [GeneratedRegex(@"\(konu `[^`]*`\)")]
    private static partial Regex SubjectNote();

    [GeneratedRegex(@"`(\w+)`")]
    private static partial Regex BacktickToken();
}
