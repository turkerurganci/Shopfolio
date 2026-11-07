namespace Shopfolio.Infrastructure.EmailComposition;

/// <summary>
/// The text rules both writers and the subject share (08 2.2.2, 2.2.4). A line break or a control character in a value becomes a blank - in the plain text
/// part, in the subject and in the sender's name (header safety: a break would start a new header) - and the ends are trimmed.
/// </summary>
internal static class EmailText
{
    private const char LineSeparator = (char)0x2028;
    private const char ParagraphSeparator = (char)0x2029;

    /// <summary>One line: every control character (the line breaks, tab, NEL) and the Unicode line and paragraph separators become a blank.</summary>
    public static string OneLine(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return string.Create(value.Length, value, static (span, source) =>
        {
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                span[i] = char.IsControl(c) || c == LineSeparator || c == ParagraphSeparator ? ' ' : c;
            }
        }).Trim();
    }

    /// <summary>The brand name as it enters the sender's name and the subject: one line, and the domain while the name is empty (02 3.29.3; 06 4.54).</summary>
    public static string BrandOrDomain(string? brandName, string domain)
    {
        string brand = brandName is null ? string.Empty : OneLine(brandName);
        return brand.Length == 0 ? domain : brand;
    }

    /// <summary>The line breaks of a text as one kind (<c>\n</c>).</summary>
    public static string NormalizeBreaks(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ReplaceLineEndings("\n");
    }

    /// <summary>A text in paragraphs (the blocks split at blank lines) of lines (the single line breaks); the HTML writer sets a paragraph per block and a break per line.</summary>
    public static IReadOnlyList<IReadOnlyList<string>> Paragraphs(string value)
    {
        List<IReadOnlyList<string>> paragraphs = [];
        List<string> lines = [];
        foreach (string line in NormalizeBreaks(value).Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (lines.Count > 0)
                {
                    paragraphs.Add(lines);
                    lines = [];
                }

                continue;
            }

            lines.Add(line);
        }

        if (lines.Count > 0)
        {
            paragraphs.Add(lines);
        }

        return paragraphs;
    }
}
