using System.Text;
using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Reader of the data model document (<c>06</c>) for the equality checks of <c>06 6</c> and <c>7</c> (T120; 09 21.4 item 1): sections by
/// heading, tables by rows, the relationship diagrams by lines. It reads text only; the document is read from the repository, the checks
/// that must fail on a changed document read a changed copy of the text (09 19.2 item 3), never the file.
/// </summary>
internal static partial class DataModelDocument
{
    public static string Load()
    {
        return File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "06_DATA_MODEL.md")).ReplaceLineEndings("\n");
    }

    /// <summary>
    /// The text of the section whose heading line starts with <paramref name="heading"/> (for example <c>### 7.1 </c>), up to the next
    /// heading of the same or a higher level.
    /// </summary>
    public static string Section(string text, string heading)
    {
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith(heading, StringComparison.Ordinal));
        if (start < 0)
        {
            throw new InvalidOperationException($"The heading '{heading}' was not found in the data model document.");
        }

        int level = heading.TakeWhile(c => c == '#').Count();
        StringBuilder section = new();
        section.Append(lines[start]).Append('\n');
        for (int i = start + 1; i < lines.Length; i++)
        {
            if (lines[i].StartsWith('#') && lines[i].TakeWhile(c => c == '#').Count() <= level && lines[i].Length > level && lines[i][level] == ' ')
            {
                break;
            }

            section.Append(lines[i]).Append('\n');
        }

        return section.ToString();
    }

    /// <summary>The tables of a text, each as its body rows (the header and the separator row are dropped); a row is its trimmed cells.</summary>
    public static IReadOnlyList<IReadOnlyList<string[]>> Tables(string text)
    {
        List<IReadOnlyList<string[]>> tables = [];
        List<string> block = [];
        foreach (string line in text.ReplaceLineEndings("\n").Split('\n').Append(string.Empty))
        {
            if (line.StartsWith('|'))
            {
                block.Add(line);
                continue;
            }

            if (block.Count > 0)
            {
                tables.Add([.. block.Skip(2).Select(Cells)]);
                block.Clear();
            }
        }

        return tables;
    }

    /// <summary>The cells of one table line: outer bars dropped, every cell trimmed.</summary>
    public static string[] Cells(string line)
    {
        string trimmed = line.Trim();
        trimmed = trimmed[1..];
        if (trimmed.EndsWith('|'))
        {
            trimmed = trimmed[..^1];
        }

        return [.. trimmed.Split('|').Select(c => c.Trim())];
    }

    /// <summary>The lines of every <c>mermaid</c> block of a text, one list per block, in order.</summary>
    public static IReadOnlyList<IReadOnlyList<string>> MermaidBlocks(string text)
    {
        List<IReadOnlyList<string>> blocks = [];
        List<string>? current = null;
        foreach (string line in text.ReplaceLineEndings("\n").Split('\n'))
        {
            if (line.StartsWith("```mermaid", StringComparison.Ordinal))
            {
                current = [];
            }
            else if (current is not null && line.StartsWith("```", StringComparison.Ordinal))
            {
                blocks.Add(current);
                current = null;
            }
            else
            {
                current?.Add(line);
            }
        }

        return blocks;
    }

    /// <summary>The code spans of a text, in order.</summary>
    public static IReadOnlyList<string> CodeSpans(string text)
    {
        return [.. CodeSpan().Matches(text).Select(m => m.Groups[1].Value)];
    }

    /// <summary>The first number group of a pattern in a text; fails when the text does not state it.</summary>
    public static int[] Numbers(string text, string pattern)
    {
        Match match = Regex.Match(text, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        if (!match.Success)
        {
            throw new InvalidOperationException($"The data model document does not state '{pattern}'.");
        }

        return [.. match.Groups.Cast<Group>().Skip(1).Select(g => int.Parse(g.Value, System.Globalization.CultureInfo.InvariantCulture))];
    }

    /// <summary>A property or column name in the lower-case underscore form the model uses for columns (<c>NormalizedSKU</c> to <c>normalized_sku</c>).</summary>
    public static string Snake(string name)
    {
        StringBuilder result = new();
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c) && i > 0)
            {
                bool previousLowerOrDigit = char.IsLower(name[i - 1]) || char.IsDigit(name[i - 1]);
                bool acronymEnd = char.IsUpper(name[i - 1]) && i + 1 < name.Length && char.IsLower(name[i + 1]);
                if (previousLowerOrDigit || acronymEnd)
                {
                    result.Append('_');
                }
            }

            result.Append(char.ToLowerInvariant(c));
        }

        return result.ToString();
    }

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex CodeSpan();
}
