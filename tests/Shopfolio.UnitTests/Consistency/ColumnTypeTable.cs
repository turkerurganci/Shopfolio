using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The storage types of <c>06 2.5.1</c> as the code knows them (T101): the type names of the document's table, the PostgreSQL type each
/// one is in the model, and the reader of the document's table. The equality check compares the document's table with
/// <see cref="Documented"/>; the model test (integration) maps the sample entity's columns through <see cref="StoreTypeOf"/>.
/// </summary>
internal static partial class ColumnTypeTable
{
    /// <summary>The type names of the table in <c>06 2.5.1</c>, as written there (second column, in code spans).</summary>
    public static readonly IReadOnlySet<string> Documented = new HashSet<string>(StringComparer.Ordinal)
    {
        "timestamptz",
        "date",
        "numeric(12,2)",
        "numeric(5,2)",
        "integer",
        "bigint",
        "numeric(12,3)",
        "varchar(n)",
        "text",
        "boolean",
        "uuid",
        "inet",
        "jsonb",
    };

    /// <summary>The type name the model reports for a documented type; <c>varchar(n)</c> is reported with its length, e.g. <c>character varying(50)</c>.</summary>
    public static string StoreTypeOf(string documented, int? length = null)
    {
        return documented switch
        {
            "timestamptz" => "timestamp with time zone",
            "varchar(n)" => $"character varying({length ?? throw new ArgumentException("varchar needs a length.", nameof(length))})",
            _ => documented,
        };
    }

    /// <summary>Reads the code-span type names from the second column of the table that follows the heading "2.5.1" in the data model document.</summary>
    public static IReadOnlySet<string> ReadDocumentedTypes(string dataModelText)
    {
        string[] lines = dataModelText.ReplaceLineEndings("\n").Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith("**2.5.1 ", StringComparison.Ordinal));
        if (start < 0)
        {
            throw new InvalidOperationException("The paragraph '2.5.1' was not found in the data model document.");
        }

        HashSet<string> types = new(StringComparer.Ordinal);
        bool inTable = false;
        for (int i = start + 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (!line.StartsWith('|'))
            {
                if (inTable)
                {
                    break;
                }

                continue;
            }

            inTable = true;
            string[] cells = line.Split('|');
            if (cells.Length < 3 || cells[1].Trim() is "Değer" || cells[1].Trim().StartsWith("---", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (Match match in CodeSpan().Matches(cells[2]))
            {
                types.Add(match.Groups[1].Value);
            }
        }

        return types;
    }

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex CodeSpan();
}
