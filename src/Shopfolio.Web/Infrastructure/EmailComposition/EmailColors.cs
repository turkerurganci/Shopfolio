namespace Shopfolio.Infrastructure.EmailComposition;

/// <summary>
/// The colours of an e-mail (08 2.2.5 item 2): the brand colour (<c>BrandColor</c>, <c>#RRGGBB</c>) is used only as the filled background of the main button, and
/// the text on it is black or white by contrast (02 3.29.5), chosen here - the higher WCAG contrast ratio wins. Everything else (links, headings, text) is the
/// product's fixed colours in the frame. A value that is not exactly <c>#</c> and six hexadecimal digits (it goes into a style attribute) is not used: the
/// button takes the product's fixed colour.
/// </summary>
public static class EmailColors
{
    public const string DefaultButtonBackground = "#1A56DB";
    public const string White = "#FFFFFF";
    public const string Black = "#000000";

    private const int HexLength = 7;

    /// <summary>The button's background and the text colour on it.</summary>
    public static (string Background, string Text) Button(string? brandColor)
    {
        if (!TryParse(brandColor, out double luminance, out string normalized))
        {
            return (DefaultButtonBackground, White);
        }

        double white = 1.05 / (luminance + 0.05);
        double black = (luminance + 0.05) / 0.05;
        return (normalized, white >= black ? White : Black);
    }

    private static bool TryParse(string? value, out double luminance, out string normalized)
    {
        luminance = 0;
        normalized = string.Empty;
        if (value is not { Length: HexLength } || value[0] != '#')
        {
            return false;
        }

        for (int i = 1; i < HexLength; i++)
        {
            if (!char.IsAsciiHexDigit(value[i]))
            {
                return false;
            }
        }

        normalized = value.ToUpperInvariant();
        luminance = (0.2126 * Linear(Convert.ToInt32(normalized.Substring(1, 2), 16)))
            + (0.7152 * Linear(Convert.ToInt32(normalized.Substring(3, 2), 16)))
            + (0.0722 * Linear(Convert.ToInt32(normalized.Substring(5, 2), 16)));
        return true;
    }

    private static double Linear(int channel)
    {
        double c = channel / 255.0;
        return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }
}
