using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The relationships of the data model document (T120; <c>06 6</c> diagrams, <c>7.2</c> foreign key table) as lines comparable with the
/// model's foreign keys, the counts the document states about them, and the reader of the index column of <c>7.2</c>.
/// </summary>
internal static partial class DocumentedRelationships
{
    /// <summary>The delete behaviors the document names, by the model's name for each (<c>06 2.2.5</c>); no other word, and in particular no RESTRICT, is a behavior.</summary>
    private static readonly Dictionary<string, string> _behaviorOfWord = new(StringComparer.Ordinal)
    {
        ["basamaklı"] = "Cascade",
        ["boşaltılır"] = "SetNull",
        ["kısıtlayıcı"] = "NoAction",
    };

    /// <summary>The diagram lines of <c>06 6.2</c>…<c>6.6</c>: each block's lines as keys (the foreign key line with the cardinality), and the lines that are not a relationship line.</summary>
    public static (IReadOnlyList<IReadOnlyList<string>> Blocks, IReadOnlyList<string> Problems) ReadDiagrams(string document)
    {
        string section = DataModelDocument.Section(document, "## 6. ");
        List<IReadOnlyList<string>> blocks = [];
        List<string> problems = [];
        foreach (IReadOnlyList<string> block in DataModelDocument.MermaidBlocks(section))
        {
            List<string> keys = [];
            foreach (string line in block.Where(l => l.Trim().Length > 0 && l.Trim() != "erDiagram"))
            {
                Match match = DiagramLine().Match(line);
                if (!match.Success)
                {
                    problems.Add($"The diagram line '{line.Trim()}' is not a relationship line of the form 'Target L--R Table : \"Field · behavior\"'.");
                    continue;
                }

                string behavior = BehaviorOf(match.Groups["behavior"].Value);
                bool required = match.Groups["left"].Value == "||";
                string one = match.Groups["right"].Value == "o|" ? "one" : "many";
                keys.Add($"{match.Groups["table"].Value}.{match.Groups["field"].Value} -> {match.Groups["target"].Value} | {(required ? "required" : "optional")} | {behavior} | {one}");
            }

            blocks.Add(keys);
        }

        return (blocks, problems);
    }

    /// <summary>The foreign key table of <c>06 7.2.1</c>: the line of each row without the cardinality and the text of its index column.</summary>
    public static (IReadOnlyList<string> Keys, IReadOnlyDictionary<string, string> IndexTextOf) ReadTable(string document)
    {
        string section = DataModelDocument.Section(document, "### 7.2 ");
        List<string> keys = [];
        Dictionary<string, string> indexText = new(StringComparer.Ordinal);
        foreach (string[] row in DataModelDocument.Tables(section)[0])
        {
            string table = DataModelDocument.CodeSpans(row[0]).Single();
            string field = DataModelDocument.CodeSpans(row[1]).Single();
            string key = $"{table}.{field} -> {row[2]} | {(row[3] == "Evet" ? "required" : "optional")} | {BehaviorOf(row[4])}";
            keys.Add(key);
            indexText[key] = row[5];
        }

        return (keys, indexText);
    }

    /// <summary>The counts the document states: the lines and rows of <c>6</c>, the count per diagram of <c>6.1.3</c>, the total of that table, and the three behavior counts of <c>7.2.1</c>.</summary>
    public static DocumentedCounts ReadCounts(string document)
    {
        string diagramSection = DataModelDocument.Section(document, "## 6. ");
        int[] lineAndRow = DataModelDocument.Numbers(diagramSection, @"(\d+) çizgi, (\d+) satır");
        IReadOnlyList<string[]> table = DataModelDocument.Tables(DataModelDocument.Section(document, "### 6.1 "))[^1];
        int[] perDiagram = [.. table.Where(r => !r[1].Contains("Toplam", StringComparison.Ordinal) && r[0].StartsWith('§')).Select(r => int.Parse(r[2], System.Globalization.CultureInfo.InvariantCulture))];
        int total = int.Parse(table.Single(r => r[1].Contains("Toplam", StringComparison.Ordinal))[2].Trim('*'), System.Globalization.CultureInfo.InvariantCulture);
        int[] behaviors = DataModelDocument.Numbers(DataModelDocument.Section(document, "### 7.2 "), @"(\d+) bağ\*{0,2} — (\d+) basamaklı, (\d+) boşaltılır, (\d+) kısıtlayıcı");
        return new DocumentedCounts(lineAndRow[0], lineAndRow[1], perDiagram, total, behaviors[0], behaviors[1], behaviors[2], behaviors[3]);
    }

    /// <summary>
    /// The foreign keys whose index the table of <c>06 7.2.1</c> promises and the model does not have (<c>06 7.2.1</c> "Dizin" column;
    /// <c>06 7.5.2</c>): the promise is read from the text of the column, the model's indexes are searched for it.
    /// </summary>
    public static IReadOnlyList<string> CheckIndexPromises(IReadOnlyList<ForeignKeyFact> foreignKeys, IReadOnlyDictionary<string, string> indexTextOf, IReadOnlyList<IndexFact> indexes)
    {
        List<string> problems = [];
        foreach (ForeignKeyFact foreignKey in foreignKeys)
        {
            if (!indexTextOf.TryGetValue(foreignKey.Key, out string? text))
            {
                continue;
            }

            string column = DataModelDocument.Snake(foreignKey.Field);
            IndexFact[] ofTable = [.. indexes.Where(i => i.Table == foreignKey.Table && i.Kind != "single-row")];
            Func<IndexFact, bool> first = i => i.Columns == column || i.Columns.StartsWith(column + ",", StringComparison.Ordinal);
            Func<IndexFact, bool> exact = i => i.Columns == column;
            Func<IndexFact, bool> plain = i => !i.Unique && i.Filter.Length == 0 && i.Kind == "plain";
            Func<IndexFact, bool> filteredUnique = i => i.Unique && i.Filter.Length > 0;
            bool ok;
            if (text.StartsWith("gerekmez", StringComparison.Ordinal))
            {
                ok = !ofTable.Any(first) && indexes.Any(i => i.Table == foreignKey.Table && i.Kind == "single-row");
            }
            else
            {
                string main = text;
                bool own = false;
                if (text.StartsWith("kendi dizini", StringComparison.Ordinal))
                {
                    own = true;
                    main = text["kendi dizini".Length..].Trim();
                }

                ok = (!own || ofTable.Any(i => exact(i) && plain(i))) && (main.Length == 0 || MainPromise(main, column, ofTable, first, exact, plain, filteredUnique));
            }

            if (!ok)
            {
                problems.Add($"{foreignKey.Key}: the document promises '{text}' and the model has no such index.");
            }
        }

        return problems;
    }

    private static bool MainPromise(string text, string column, IndexFact[] ofTable, Func<IndexFact, bool> first, Func<IndexFact, bool> exact, Func<IndexFact, bool> plain, Func<IndexFact, bool> filteredUnique)
    {
        string t = text.StartsWith("ve ", StringComparison.Ordinal) ? text[3..] : text;
        if (t.StartsWith('('))
        {
            string[] columns = DocumentedIndexes.Columns(t);
            return ofTable.Any(i => i.Columns == string.Join(",", columns) && first(i) && (!t.Contains("tekil", StringComparison.Ordinal) || i.Unique));
        }

        return t switch
        {
            "tekil dizini" => ofTable.Any(i => exact(i) && i.Unique && i.Filter.Length == 0),
            "filtreli tekil dizini" => ofTable.Any(i => exact(i) && filteredUnique(i)),
            "filtreli tekil dizini ve düz dizini" => ofTable.Any(i => exact(i) && filteredUnique(i)) && ofTable.Any(i => exact(i) && plain(i)),
            "çiftin tekil dizininin ilk sütunu" => ofTable.Any(i => first(i) && i.Unique && i.Filter.Length == 0 && i.Columns.Split(',').Length == 2),
            "kombinasyonun tekil dizininin ilk sütunu" => ofTable.Any(i => first(i) && i.Unique && i.Filter.Length == 0 && i.Columns.Split(',').Length == 3),
            "iki filtreli tekil dizinin ilk sütunu" => ofTable.Count(i => first(i) && filteredUnique(i)) >= 2,
            "açık isteklerin filtreli tekil dizini" or "kalemsiz açık isteklerin filtreli tekil dizini" => ofTable.Any(i => first(i) && filteredUnique(i)),
            _ => throw new InvalidOperationException($"The index promise '{text}' of the foreign key table (06 7.2.1) is not a promise this reader knows; add it with its check."),
        };
    }

    private static string BehaviorOf(string word)
    {
        return _behaviorOfWord.TryGetValue(word.Trim(), out string? behavior) ? behavior : word.Trim();
    }

    [GeneratedRegex("""^\s*(?<target>\w+) (?<left>\|\||\|o)--(?<right>o\{|o\|) (?<table>\w+) : "(?<field>\w+) · (?<behavior>[^"]+)"\s*$""", RegexOptions.CultureInvariant)]
    private static partial Regex DiagramLine();
}

/// <summary>The counts the data model document states about the relationships.</summary>
internal sealed record DocumentedCounts(int Lines, int Rows, int[] PerDiagram, int Total, int Relationships, int Cascade, int SetNull, int Restrictive);
