using System.Text;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The search word of a list as a <c>LIKE</c> pattern (09 10.5 item 5; 07 2.6.3). One function, in this order: trim, <see cref="TextNormalizer.Simplify"/>
/// (the word reaches the form the searched column has), cut to <see cref="ProductConstants.ProductNameMaxLength"/> characters (a cut through a surrogate
/// pair drops the pair's other half too), put <c>\</c> before <c>\</c>, <c>%</c> and <c>_</c>, put <c>%</c> at both ends. The cut comes before the escape: a cut
/// after it could split a <c>\%</c> pair. A word that is empty or only blanks - and so is empty after the fold - makes no result list: <see langword="null"/>,
/// and the page shows the search field (07 2.6.3; 04 7.1.1.3). The pattern is a query parameter; the word is never written into the query text. The escape
/// character is <see cref="EscapeCharacter"/> and the query names it: <c>EF.Functions.Like(column, pattern, "\\")</c>.
/// </summary>
internal static class SearchTerm
{
    /// <summary>The escape character of the pattern; the third argument of <c>EF.Functions.Like</c>.</summary>
    public const string EscapeCharacter = "\\";

    public static string? ToLikePattern(string? raw)
    {
        if (raw is null)
        {
            return null;
        }

        string word = TextNormalizer.Simplify(raw.Trim());
        if (word.Length == 0)
        {
            return null;
        }

        if (word.Length > ProductConstants.ProductNameMaxLength)
        {
            int length = ProductConstants.ProductNameMaxLength;
            if (char.IsHighSurrogate(word[length - 1]))
            {
                length--;
            }

            word = word[..length];
        }

        StringBuilder pattern = new(word.Length + 4);
        pattern.Append('%');
        foreach (char c in word)
        {
            if (c is '\\' or '%' or '_')
            {
                pattern.Append('\\');
            }

            pattern.Append(c);
        }

        return pattern.Append('%').ToString();
    }
}
