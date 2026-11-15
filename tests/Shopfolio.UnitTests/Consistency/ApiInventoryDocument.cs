using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>A field of a documented model: its name and - for a list or a group - the fields it carries (07 5.1.5: "kalem başına …").</summary>
internal sealed class DocField(string name)
{
    public string Name { get; } = name;

    public List<DocField> Children { get; } = [];
}

/// <summary>A row of a model table of 07 5.2, 5.3.2 or 5.4.2: the model names (a row may name two), the use cases and the fields the row names.</summary>
internal sealed record ModelRow(IReadOnlyList<string> Models, IReadOnlyList<string> UseCases, IReadOnlyList<DocField> Fields);

/// <summary>One address-and-handler of a row of the endpoint inventory (07 3.2): <c>GET /sepet</c>, <c>POST /odeme?handler=Leave</c>.</summary>
/// <param name="UseCase">The row's id, <c>UC-27</c>.</param>
/// <param name="Method">GET or POST.</param>
/// <param name="Path">The path as the document writes it; <c>null</c> when the row names the handler only (UC-117: the address is the form's own).</param>
/// <param name="Handler">The <c>handler</c> parameter, or <c>null</c>.</param>
internal sealed record EndpointRef(string UseCase, string Method, string? Path, string? Handler);

/// <summary>
/// Reads the model tables and the endpoint inventory of <c>07</c> (T538; 07 3.2, 5.2, 5.3.2, 5.4.2; 09 21.4 item 1). The readers take the document's text, so the
/// checks can be shown to fail on a deliberately wrong text (09 19.2 item 3). A field is the backticked name at the head of a segment of the "Alanlar" cell
/// (segments are separated by " · "); a name after "başına", after "ya da" or "ve" at the start of a part, and the name that follows a comma in the explanation
/// part are fields too; the name of a value (<c>Draft</c> ya da <c>Published</c>) is not.
/// </summary>
internal static partial class ApiInventoryDocument
{
    /// <summary>The names the document writes in backticks inside an explanation without their being fields (07 5.2.1 <c>AcceptedCheckboxes</c>: the values of <c>CheckboxType</c>).</summary>
    private static readonly HashSet<string> NotFields = new(StringComparer.Ordinal) { "CheckboxType" };

    public static string Text() =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "07_API_DESIGN.md")).ReplaceLineEndings("\n");

    /// <summary>The form input rows of 07 5.2.1 and 5.2.2.</summary>
    public static IReadOnlyList<ModelRow> FormRows(string text) => Rows(text, "### 5.2 Form girdi modelleri", "**5.2.3 ", pascal: true);

    /// <summary>The query model rows of 07 5.3.2.</summary>
    public static IReadOnlyList<ModelRow> QueryRows(string text) => Rows(text, "**5.3.2 Modeller.**", "Ana sayfanın sayaçları", pascal: false);

    /// <summary>The machine body rows of 07 5.4.2 (the second column is the direction, not the use cases; the use case is UC-83's).</summary>
    public static IReadOnlyList<ModelRow> MachineRows(string text) => Rows(text, "**5.4.2 Parçalı yükleme**", "Parçanın gövdesi", pascal: false);

    /// <summary>The use cases 07 5.2.4 names as "alan taşımayan işleyiciler modelsizdir".</summary>
    public static IReadOnlyList<string> ModellessUseCases(string text)
    {
        string[] lines = text.Split('\n');
        string paragraph = Assert.Single(lines, l => l.StartsWith("**5.2.4 Alan taşımayan işleyiciler modelsizdir**", StringComparison.Ordinal));
        int end = paragraph.IndexOf("Ödeme sağlayıcısından dönüşün", StringComparison.Ordinal);
        Assert.True(end > 0, "07 5.2.4 no longer ends its list with the sentence about the provider's return.");
        return [.. UseCase().Matches(paragraph[..end]).Select(m => m.Value).Distinct(StringComparer.Ordinal)];
    }

    /// <summary>The rows of the endpoint inventory, 07 3.2, as address-and-handler references.</summary>
    public static IReadOnlyList<EndpointRef> Endpoints(string text)
    {
        string[] lines = text.Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith("### 3.2 Envanter", StringComparison.Ordinal));
        int end = Array.FindIndex(lines, l => l.StartsWith("### 3.3 ", StringComparison.Ordinal));
        Assert.True(start >= 0 && end > start, "07 3.2 is not found.");

        List<EndpointRef> endpoints = [];
        foreach (string line in lines[start..end].Where(l => l.StartsWith("| UC-", StringComparison.Ordinal)))
        {
            string[] cells = line.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
            string useCase = cells[0];
            string? method = null;
            string? lastPath = null;
            foreach (string piece in cells[1].Split(" · "))
            {
                Match match = EndpointPiece().Match(piece);
                if (!match.Success)
                {
                    continue;
                }

                method = match.Groups["method"].Success ? match.Groups["method"].Value : method;
                string address = match.Groups["address"].Value;
                int query = address.IndexOf('?', StringComparison.Ordinal);
                string? path = query < 0 ? address : address[..query];
                string? handler = null;
                if (query >= 0 && address[(query + 1)..].StartsWith("handler=", StringComparison.Ordinal))
                {
                    handler = address[(query + 1 + "handler=".Length)..];
                }

                if (string.IsNullOrEmpty(path))
                {
                    path = lastPath;
                }
                else if (path.StartsWith('…'))
                {
                    path = null;
                }

                lastPath = path ?? lastPath;
                endpoints.Add(new EndpointRef(useCase, method ?? "GET", path, handler));
            }
        }

        return endpoints;
    }

    private static List<ModelRow> Rows(string text, string startMarker, string endMarker, bool pascal)
    {
        string[] lines = text.Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith(startMarker, StringComparison.Ordinal));
        Assert.True(start >= 0, $"'{startMarker}' is not found in 07.");
        int end = Array.FindIndex(lines, start + 1, l => l.StartsWith(endMarker, StringComparison.Ordinal));
        Assert.True(end > start, $"'{endMarker}' does not follow '{startMarker}' in 07.");

        List<ModelRow> rows = [];
        foreach (string line in lines[start..end].Where(l => l.StartsWith("| `", StringComparison.Ordinal)))
        {
            string[] cells = [.. line.Trim().Trim('|').Split('|').Select(c => c.Trim())];
            Assert.Equal(3, cells.Length);
            string[] models = [.. Token().Matches(cells[0]).Select(m => m.Groups["t"].Value)];
            string[] useCases = startMarker.StartsWith("**5.4.2", StringComparison.Ordinal) ? ["UC-83"] : [.. UseCase().Matches(cells[1]).Select(m => m.Value)];
            rows.Add(new ModelRow(models, useCases, ParseFields(cells[2], pascal)));
        }

        // The rows that point to another row's fields ("yedi bağdan biri" without the list; "MarkShippedInput'un kargo alanları").
        for (int i = 0; i < rows.Count; i++)
        {
            rows[i] = new ModelRow(rows[i].Models, rows[i].UseCases, ResolveReferences(rows[i].Fields, rows));
        }

        return rows;
    }

    private static List<DocField> ResolveReferences(IReadOnlyList<DocField> fields, List<ModelRow> rows)
    {
        List<DocField> resolved = [];
        foreach (DocField field in fields)
        {
            if (!field.Name.StartsWith('@'))
            {
                resolved.Add(field);
                continue;
            }

            // "@Model|what" - the fields of the named model's row; "kargo alanları" is the first group of the row, "alanları" the whole row.
            string[] reference = field.Name[1..].Split('|');
            ModelRow? row = rows.FirstOrDefault(r => r.Models.Contains(reference[0]));
            Assert.True(row is not null, $"07 5.2 refers to the row of '{reference[0]}', which is not a row.");
            IEnumerable<DocField> taken = reference[1] switch
            {
                "kargo" => row.Fields.Take(4),
                "links" => row.Fields.Take(7),
                _ => row.Fields,
            };
            resolved.AddRange(taken.Where(f => !f.Name.StartsWith('@')));
        }

        return resolved;
    }

    private static List<DocField> ParseFields(string cell, bool pascal)
    {
        string text = cell.Replace("**", string.Empty, StringComparison.Ordinal);
        string previous;
        do
        {
            previous = text;
            text = Parenthesis().Replace(text, string.Empty);
        }
        while (previous != text);

        text = text.Replace("UC-117'nin iki alanı", "`ImageFile`, `ImageTarget`", StringComparison.Ordinal);

        List<DocField> fields = [];
        foreach (string segment in text.Split(" · "))
        {
            string[] parts = segment.Split(" — ");
            Match reference = ModelReference().Match(parts[0]);
            if (reference.Success)
            {
                fields.Add(new DocField($"@{reference.Groups["m"].Value}|{(parts[0].Contains("kargo alanları", StringComparison.Ordinal) ? "kargo" : "all")}"));
                continue;
            }

            List<DocField> head = [.. Names(parts[0], pascal).Select(n => new DocField(n))];
            if (head.Count == 0)
            {
                // "tipin alanları — Hakkımızda `A`, `B`" and "kaydın bağı — yedi bağdan biri: `A`, …": the tail is the list.
                head = [.. Names(string.Join(' ', parts.Skip(1)), pascal).Select(n => new DocField(n))];
                if (head.Count == 0 && segment.Contains("yedi bağdan biri", StringComparison.Ordinal))
                {
                    head.Add(new DocField("@ProcessRefundInput|links"));
                }

                fields.AddRange(head);
                continue;
            }

            fields.AddRange(head);
            DocField current = head[^1];
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                int each = part.IndexOf("başına ", StringComparison.Ordinal);
                if (each >= 0)
                {
                    List<DocField> children = [.. Names(part[(each + "başına ".Length)..], pascal).Select(n => new DocField(n))];
                    current.Children.AddRange(children);
                    current = children.Count > 0 ? children[^1] : current;
                }
                else if (part.StartsWith("ya da `", StringComparison.Ordinal) || part.StartsWith("ve `", StringComparison.Ordinal))
                {
                    fields.AddRange(Names(part, pascal).Select(n => new DocField(n)));
                }
                else if (part.StartsWith('`') && !part.Contains(" ya da ", StringComparison.Ordinal) && pascal)
                {
                    current.Children.AddRange(Names(part, pascal).Select(n => new DocField(n)));
                }
                else if (pascal)
                {
                    fields.AddRange(AfterComma().Matches(part).Select(m => m.Groups["t"].Value).Where(n => !NotFields.Contains(n)).Select(n => new DocField(n)));
                }
            }
        }

        return fields;
    }

    private static IEnumerable<string> Names(string text, bool pascal) =>
        Token().Matches(text).Where(m => !m.Value.EndsWith("'", StringComparison.Ordinal))
            .Select(m => m.Groups["t"].Value)
            .Where(n => pascal ? char.IsUpper(n[0]) : char.IsLower(n[0]))
            .Where(n => !NotFields.Contains(n));

    [GeneratedRegex(@"`(?<t>[A-Za-z][A-Za-z0-9]*)`'?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Token();

    [GeneratedRegex(@"\([^()]*\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Parenthesis();

    [GeneratedRegex(@"`(?<m>[A-Za-z]+Input)`'un ", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ModelReference();

    [GeneratedRegex(@", `(?<t>[A-Z][A-Za-z0-9]*)`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AfterComma();

    [GeneratedRegex(@"UC-\d+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex UseCase();

    [GeneratedRegex(@"^(?:(?<method>GET|POST) )?`(?<address>[^`]*)`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EndpointPiece();
}
