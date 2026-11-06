using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.Infrastructure.SearchEngine;

/// <summary>
/// The texts the search engines read, made from the record (02 3.30.5; 05 3.7.2): the page title and the description. Nothing here is typed by the firm
/// - there is no SEO field (02 3.30.5; KD-48) - and every function is pure, so a page and a test call the same code.
/// </summary>
internal static partial class SearchText
{
    /// <summary>The separator of the title: the record's name, an en dash, the brand name ("Ahşap Masa – Yılmaz Mobilya"; 02 3.30.5 item 1).</summary>
    public const string TitleSeparator = " – ";

    /// <summary>
    /// The page title: the name of the record and the brand name. A closed screen's tab title is built the same way from the screen's name and the brand name
    /// (02 3.30.5 item 1; K-1559). A brand name that is not filled yet leaves the name alone.
    /// </summary>
    public static string Title(string name, string? brandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        string brand = brandName?.Trim() ?? string.Empty;
        return brand.Length == 0 ? name.Trim() : name.Trim() + TitleSeparator + brand;
    }

    /// <summary>
    /// The description of a record page (02 3.30.5 item 2): the record's short description or short introduction when it has one; a page without a short
    /// field takes the start of its text in plain form (<see cref="PlainText.FromHtml"/>). Either way it is cut at <see cref="ProductConstants.ShortTextMaxLength"/>
    /// characters without cutting a word in the middle. <see langword="null"/> when the record has neither.
    /// </summary>
    public static string? Description(string? shortText, string? bodyHtml)
    {
        // The short fields are plain text (P-28); only the body is formatted text.
        string plainShort = shortText is null ? string.Empty : Whitespace().Replace(shortText, " ").Trim();
        if (plainShort.Length > 0)
        {
            return CutAtWord(plainShort, ProductConstants.ShortTextMaxLength);
        }

        string plainBody = PlainText.FromHtml(bodyHtml);
        return plainBody.Length == 0 ? null : CutAtWord(plainBody, ProductConstants.ShortTextMaxLength);
    }

    /// <summary>
    /// The description of a category page (02 3.30.5 item 2): made from the category's name and the brand name. The sentence is the application's own copy;
    /// the name is cut so that the whole stays within the ceiling.
    /// </summary>
    public static string CategoryDescription(string categoryName, string? brandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);
        string brand = brandName?.Trim() ?? string.Empty;
        string sentence = brand.Length == 0
            ? $"{categoryName.Trim()} kategorisindeki ürünleri inceleyin."
            : $"{categoryName.Trim()} kategorisindeki ürünleri {brand} sitesinde inceleyin.";
        return CutAtWord(sentence, ProductConstants.ShortTextMaxLength);
    }

    /// <summary>
    /// Cuts <paramref name="text"/> to at most <paramref name="max"/> characters at the end of a word (02 3.30.5; K-1558): when the character after the cut is
    /// not a space, the cut moves back to the last space; a first word longer than the ceiling is cut at the ceiling (a surrogate pair is not split). Text
    /// that fits is returned as it is. The ending is trimmed of spaces.
    /// </summary>
    public static string CutAtWord(string text, int max)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 1);
        if (text.Length <= max)
        {
            return text;
        }

        int cut = max;
        if (char.IsHighSurrogate(text[cut - 1]))
        {
            cut--;
        }

        if (text[cut] != ' ')
        {
            int space = text.LastIndexOf(' ', cut - 1, cut);
            if (space > 0)
            {
                cut = space;
            }
        }

        return text[..cut].TrimEnd();
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    internal static partial Regex Whitespace();
}

/// <summary>The plain form of a formatted text (02 3.30.5 item 2: "metnin biçimlendirmesiz düz hâli"): the text without tags, entities decoded, white space single.</summary>
internal static partial class PlainText
{
    private static readonly string[] _spacingTags =
    [
        "p", "br", "div", "li", "ul", "ol", "h1", "h2", "h3", "h4", "h5", "h6", "tr", "td", "th", "table", "blockquote", "pre", "hr",
    ];

    /// <summary>
    /// The text of <paramref name="html"/> as plain text. The content of a <c>script</c> or <c>style</c> element is dropped; a block or line-break tag is a
    /// space (so two paragraphs do not run into one word); every other tag is removed; entities are decoded; runs of white space become one space; the ends are
    /// trimmed. An empty or white-space value gives the empty string. The sanitized rich text (09 15.3) is the input, but nothing here relies on that: a tag
    /// left in the text is data to the reader of the result, which the page encodes on output.
    /// </summary>
    public static string FromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        string withoutBlocks = DroppedElements().Replace(html, " ");
        StringBuilder text = new(withoutBlocks.Length);
        int position = 0;
        foreach (Match tag in Tag().Matches(withoutBlocks))
        {
            text.Append(withoutBlocks, position, tag.Index - position);
            string name = tag.Groups["name"].Value.ToLowerInvariant();
            if (Array.IndexOf(_spacingTags, name) >= 0)
            {
                text.Append(' ');
            }

            position = tag.Index + tag.Length;
        }

        text.Append(withoutBlocks, position, withoutBlocks.Length - position);
        string decoded = WebUtility.HtmlDecode(text.ToString());
        return SearchText.Whitespace().Replace(decoded, " ").Trim();
    }

    [GeneratedRegex(@"<(script|style)\b[^>]*>.*?</\1\s*>|<!--.*?-->", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DroppedElements();

    [GeneratedRegex(@"</?(?<name>[A-Za-z][A-Za-z0-9]*)\b[^>]*>", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Tag();
}
