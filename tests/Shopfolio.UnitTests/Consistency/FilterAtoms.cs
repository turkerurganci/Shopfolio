using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The filter of a filtered index in one comparable form: the conditions joined by AND, each in the model's own SQL spelling
/// (<c>col IS NULL</c>, <c>col IS NOT NULL</c>, <c>col</c>, <c>col = 'X'</c>, <c>col IN ('A', 'B')</c>), sorted. The model's filter is
/// split into its conditions; the document states a filter in words (<c>06 7.1</c>, <c>7.5.1</c>), which the readers below turn into the
/// same conditions. A word the reader does not know is a problem, not a skipped filter.
/// </summary>
internal static partial class FilterAtoms
{
    /// <summary>The phrases of the query index table (<c>06 7.5.1</c>) that name a filter; the longer phrase is tried first and is consumed. <c>{c}</c> is the index's first column.</summary>
    private static readonly (string Phrase, string Sql)[] _queryPhrases =
    [
        ("`PreviousEmail` dolu satırlarda filtreli", "previous_email IS NOT NULL"),
        ("işaretli ve yayındaki satırlarda filtreli", "{c} AND publish_status = 'Published'"),
        ("ödemesi beklenen siparişlerde filtreli", "payment_status = 'Pending'"),
        ("tamamlanmamış yüklemelerde filtreli", "upload_completed_at IS NULL"),
        ("tamamlanmış satırlarda filtreli", "job_state = 'Completed'"),
        ("misafir sepetlerinde filtreli", "guest_cart_key_hash IS NOT NULL"),
        ("\"gönderilecek\" satırlarda filtreli", "dispatch_state = 'Queued'"),
        ("başarısız satırlarda filtreli", "job_state = 'Failed'"),
        ("bekliyor satırlarda filtreli", "job_state = 'Pending'"),
        ("`Open` satırlarda filtreli", "defect_claim_status = 'Open'"),
        ("dolu satırlarda filtreli", "{c} IS NOT NULL"),
    ];

    /// <summary>The model's filter, split into its AND conditions and sorted; empty when the index has no filter. A filter with OR is kept whole.</summary>
    public static string FromSql(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return string.Empty;
        }

        string[] atoms = sql.Contains(" OR ", StringComparison.Ordinal) ? [sql.Trim()] : sql.Split(" AND ", StringSplitOptions.TrimEntries);
        return string.Join(" AND ", atoms.Order(StringComparer.Ordinal));
    }

    /// <summary>
    /// The filter of the unique index table (<c>06 7.1</c>): a name followed by <c>dolu</c> (not null), <c>boş</c> (null) or <c>doğru</c>
    /// (true); names joined by a comma or <c>ve</c> before one word share it; <c>dolu satırlar</c> is the index column itself; a name
    /// directly followed by value names is an equality (<c>`State` `Succeeded`</c>) or, with <c>ya da</c>, a list. Returns null for a text
    /// the reader does not understand.
    /// </summary>
    public static string? FromUniqueProse(string prose, string firstColumn)
    {
        string text = prose.Trim();
        if (text is "—" or "")
        {
            return string.Empty;
        }

        if (text == "dolu satırlar")
        {
            return FromSql($"{firstColumn} IS NOT NULL");
        }

        List<string> atoms = [];
        List<string> names = [];
        List<string> values = [];
        int previousEnd = 0;
        bool previousWasSpan = false;
        bool afterOr = false;
        foreach (Match token in Token().Matches(text))
        {
            bool adjacent = text[previousEnd..token.Index].Trim().Length == 0;
            previousEnd = token.Index + token.Length;
            if (token.Value.StartsWith('`'))
            {
                string name = token.Value.Trim('`');
                if ((previousWasSpan && adjacent && names.Count > 0) || (afterOr && values.Count > 0))
                {
                    values.Add(name);
                }
                else
                {
                    names.Add(name);
                }

                previousWasSpan = true;
                afterOr = false;
                continue;
            }

            previousWasSpan = false;
            afterOr = token.Value == "ya da";
            string? predicate = token.Value switch
            {
                "dolu" => "{0} IS NOT NULL",
                "boş" => "{0} IS NULL",
                "doğru" => "{0}",
                _ => null,
            };
            if (predicate is not null && values.Count == 0)
            {
                atoms.AddRange(names.Select(n => string.Format(System.Globalization.CultureInfo.InvariantCulture, predicate, Snake(n))));
                names.Clear();
            }
        }

        if (values.Count > 0 && names.Count == 1)
        {
            string column = Snake(names[0]);
            atoms.Add(values.Count == 1 ? $"{column} = '{values[0]}'" : $"{column} IN ({string.Join(", ", values.Select(v => $"'{v}'"))})");
            names.Clear();
        }

        return names.Count == 0 && atoms.Count > 0 ? FromSql(string.Join(" AND ", atoms)) : null;
    }

    /// <summary>
    /// The filter words of the query index text (<c>06 7.5.1</c>): the known phrases are consumed and turned into conditions; a text that
    /// still says <c>filtreli</c> after that holds a phrase this reader does not know (the second item).
    /// </summary>
    public static (string Filter, bool Unknown) FromQueryProse(string text, string firstColumn)
    {
        List<string> atoms = [];
        string rest = text;
        foreach ((string phrase, string sql) in _queryPhrases)
        {
            if (rest.Contains(phrase, StringComparison.Ordinal))
            {
                atoms.AddRange(sql.Replace("{c}", firstColumn, StringComparison.Ordinal).Split(" AND ", StringSplitOptions.TrimEntries));
                rest = rest.Replace(phrase, string.Empty, StringComparison.Ordinal);
            }
        }

        return (FromSql(string.Join(" AND ", atoms)), rest.Contains("filtreli", StringComparison.Ordinal));
    }

    private static string Snake(string name) => DataModelDocument.Snake(name);

    [GeneratedRegex("`[^`]+`|ya da|dolu|boş|doğru", RegexOptions.CultureInvariant)]
    private static partial Regex Token();
}
