namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The index tables of the data model document as index facts (T120; <c>06 7.1</c> unique indexes, <c>7.5.1</c> query indexes) and the
/// comparison of a documented set with the index set of the database. Both directions are checked: an index only in the document and an
/// index only in the model are both problems.
/// </summary>
internal static class DocumentedIndexes
{
    /// <summary>The unique index table of <c>06 7.1</c>.</summary>
    public static (IReadOnlyList<IndexFact> Facts, IReadOnlyList<string> Problems) ReadUnique(string document)
    {
        string section = DataModelDocument.Section(document, "### 7.1 ");
        IReadOnlyList<string[]> rows = DataModelDocument.Tables(section)[0];
        List<IndexFact> facts = [];
        List<string> problems = [];
        foreach (string[] row in rows)
        {
            string table = DataModelDocument.CodeSpans(row[0]).Single();
            if (row[1].StartsWith("Tek satır", StringComparison.Ordinal))
            {
                facts.Add(new IndexFact(table, "(true)", true, string.Empty, false, "single-row"));
                continue;
            }

            string[] columns = Columns(row[1]);
            string? filter = FilterAtoms.FromUniqueProse(row[2], columns[0]);
            if (filter is null)
            {
                problems.Add($"{table} ({string.Join(",", columns)}): the filter '{row[2]}' is not a filter the reader of 06 7.1 understands.");
                continue;
            }

            facts.Add(new IndexFact(table, string.Join(",", columns), true, filter, row[1].Contains("NULLS NOT DISTINCT", StringComparison.Ordinal), "plain"));
        }

        return (facts, problems);
    }

    /// <summary>The query index table of <c>06 7.5.1</c>; a row naming several tables is one index on each.</summary>
    public static (IReadOnlyList<IndexFact> Facts, IReadOnlyList<string> Problems) ReadQuery(string document)
    {
        string section = DataModelDocument.Section(document, "### 7.5 ");
        IReadOnlyList<string[]> rows = DataModelDocument.Tables(section)[0];
        List<IndexFact> facts = [];
        List<string> problems = [];
        foreach (string[] row in rows)
        {
            string[] columns = Columns(row[1]);
            (string filter, bool unknown) = FilterAtoms.FromQueryProse(row[1], columns[0]);
            if (unknown)
            {
                problems.Add($"{row[0]} ({string.Join(",", columns)}): the text '{row[1]}' names a filter the reader of 06 7.5.1 does not know.");
            }

            string kind = row[1].Contains("trigram", StringComparison.Ordinal) ? "trigram" : row[1].Contains("ifade dizini", StringComparison.Ordinal) ? "expression" : "plain";
            foreach (string table in DataModelDocument.CodeSpans(row[0]))
            {
                facts.Add(new IndexFact(table, string.Join(",", columns), false, filter, false, kind));
            }
        }

        return (facts, problems);
    }

    /// <summary>The columns an index text names: the code spans inside the first parentheses, or the first code span.</summary>
    public static string[] Columns(string text)
    {
        string source = text;
        if (text.StartsWith('('))
        {
            int close = text.IndexOf("`)", StringComparison.Ordinal);
            source = close < 0 ? text : text[..(close + 2)];
        }

        IReadOnlyList<string> spans = DataModelDocument.CodeSpans(source);
        return [.. (text.StartsWith('(') ? spans : spans.Take(1)).Select(s => DataModelDocument.Snake(s).Replace(", ", ",", StringComparison.Ordinal))];
    }

    /// <summary>The differences between a documented and a model set, one line per index; empty when the sets are equal.</summary>
    public static IReadOnlyList<string> Compare(string label, IEnumerable<IndexFact> documented, IEnumerable<IndexFact> model)
    {
        return SetComparison.Compare(label, documented.Select(f => f.Key), model.Select(f => f.Key));
    }
}
