using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The check constraint part of the data model document (T120; <c>06 7.3</c>) and the sorting of the model's constraints into the three
/// kinds the document treats as common (enum value sets, the ten-digit phone, the page address format) and the table-specific rest.
/// </summary>
internal static partial class DocumentedChecks
{
    /// <summary>The galleries' link tables, which the document names as "galeri bağları" in its list of tables without a specific constraint.</summary>
    public static readonly string[] GalleryLinkTables = ["ProductImage", "AboutPageImage", "OfferingImage", "PortfolioItemImage", "FaqItemImage", "CustomPageImage"];

    public static bool IsEnumSet(CheckFact check) => EnumSet().IsMatch(check.Sql);

    public static bool IsPhone(CheckFact check) => check.Sql.EndsWith("~ '^[0-9]{10}$'", StringComparison.Ordinal);

    public static bool IsSlug(CheckFact check) => check.Name.EndsWith("_slug_format", StringComparison.Ordinal);

    public static IEnumerable<CheckFact> Specific(IEnumerable<CheckFact> checks) => checks.Where(c => !IsEnumSet(c) && !IsPhone(c) && !IsSlug(c));

    /// <summary>The rows of <c>06 7.3.2</c> except the page address row: per table the number of items its row states.</summary>
    public static IReadOnlyDictionary<string, int> ReadRows(string document)
    {
        Dictionary<string, int> rows = new(StringComparer.Ordinal);
        foreach (string[] row in SpecificTable(document).Where(r => !r[1].StartsWith("`Slug`", StringComparison.Ordinal)))
        {
            foreach (string table in DataModelDocument.CodeSpans(row[0]))
            {
                rows[table] = Items(row[1]).Count;
            }
        }

        return rows;
    }

    /// <summary>The tables of the page address row of <c>06 7.3.2</c>.</summary>
    public static IReadOnlyList<string> ReadSlugTables(string document)
    {
        string[] row = SpecificTable(document).Single(r => r[1].StartsWith("`Slug`", StringComparison.Ordinal));
        return DataModelDocument.CodeSpans(row[0]);
    }

    /// <summary>The tables the document says carry no specific constraint (the paragraph after the table of <c>06 7.3.2</c>); the galleries' link tables are named as a group in it.</summary>
    public static IReadOnlyList<string> ReadWithoutSpecific(string document)
    {
        string section = DataModelDocument.Section(document, "### 7.3 ");
        string paragraph = section.Split('\n').Single(l => l.StartsWith("Kendine özgü kontrol kısıtı taşımayan tablolar", StringComparison.Ordinal));
        List<string> tables = [.. DataModelDocument.CodeSpans(paragraph)];
        if (paragraph.Contains("galeri bağları", StringComparison.Ordinal))
        {
            tables.AddRange(GalleryLinkTables);
        }

        return tables;
    }

    /// <summary>The phones of the bullet of <c>06 7.3.1</c>, as <c>Entity.Property</c>; a code span without an entity continues the entity before it.</summary>
    public static IReadOnlyList<string> ReadPhones(string document)
    {
        string section = DataModelDocument.Section(document, "### 7.3 ");
        string bullet = section.Split('\n').Single(l => l.StartsWith("- **Telefon**", StringComparison.Ordinal));
        List<string> phones = [];
        string? entity = null;
        foreach (string span in DataModelDocument.CodeSpans(bullet))
        {
            string[] parts = span.Split('.');
            if (parts.Length == 2)
            {
                entity = parts[0];
                phones.Add(span);
            }
            else if (entity is not null)
            {
                phones.Add($"{entity}.{span}");
            }
        }

        return phones;
    }

    /// <summary>The phone of a model constraint as <c>Entity.Property</c>, found by the column the constraint names.</summary>
    public static string PhoneOf(CheckFact check, IEnumerable<PropertyFact> properties)
    {
        string column = check.Sql.Split(' ')[0];
        return $"{check.Table}.{properties.Single(p => p.Table == check.Table && DataModelDocument.Snake(p.Name) == column).Name}";
    }

    /// <summary>The items of a row's text: split at a semicolon outside parentheses.</summary>
    public static IReadOnlyList<string> Items(string text)
    {
        List<string> items = [];
        int depth = 0;
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '(')
            {
                depth++;
            }
            else if (text[i] == ')')
            {
                depth--;
            }
            else if (text[i] == ';' && depth == 0)
            {
                items.Add(text[start..i].Trim());
                start = i + 1;
            }
        }

        if (text[start..].Trim().Length > 0)
        {
            items.Add(text[start..].Trim());
        }

        return items;
    }

    private static IReadOnlyList<string[]> SpecificTable(string document)
    {
        string section = DataModelDocument.Section(document, "### 7.3 ");
        return DataModelDocument.Tables(section)[0];
    }

    [GeneratedRegex(@"^""[a-z0-9_]+"" IN \(('[^']*'(, )?)+\)$", RegexOptions.CultureInvariant)]
    private static partial Regex EnumSet();
}
