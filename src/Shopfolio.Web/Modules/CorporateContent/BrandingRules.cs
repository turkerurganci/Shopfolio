using System.Globalization;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The rules of the brand identity that need no database (T362; 02 3.29; 06 4.54). Pure: no clock, no row. The colour the text on the brand colour takes is not decided here - the one
/// rule of the product for it is <c>EmailColors.Button</c> (02 3.29.5, 08 2.2.5), which the storefront and the e-mail share.
/// </summary>
internal static class BrandingRules
{
    /// <summary>The longest value a trail row holds (<c>AuditEntryRules.MaxValueLength</c>; a test keeps the two equal): the brand name has no ceiling of its own (06 4.54), so its trail text stops here.</summary>
    public const int TrailValueMaxLength = 2000;

    private const int ColorLength = 7;

    /// <summary>The brand name as it is stored: trimmed; empty or white space is none (06 4.54).</summary>
    public static string? NormalizeName(string? raw) => string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();

    /// <summary>
    /// The colour as it is stored (06 4.54): <c>#</c> and six hexadecimal digits in capitals. Blank is no colour and is accepted (the product's default applies); anything else that is not exactly
    /// that shape - it goes into a style attribute of the page - is refused. A colour is the only value a firm gives that reaches a style.
    /// </summary>
    public static bool TryNormalizeColor(string? raw, out string? color)
    {
        color = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        string trimmed = raw.Trim();
        if (trimmed.Length != ColorLength || trimmed[0] != '#' || !trimmed.Skip(1).All(char.IsAsciiHexDigit))
        {
            return false;
        }

        color = trimmed.ToUpperInvariant();
        return true;
    }

    /// <summary>The first letter of the displayed name, in capitals (02 3.29.4): the first text element, upper-cased the way the product writes Turkish.</summary>
    public static string Initial(string displayName)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        string trimmed = displayName.Trim();
        return trimmed.Length == 0 ? string.Empty : StringInfo.GetNextTextElement(trimmed).ToUpper(Shopfolio.Infrastructure.RequestFormat.Culture);
    }

    /// <summary>A value as the trail shows it: at most <see cref="TrailValueMaxLength"/> characters.</summary>
    public static string? TrailText(string? value) => value is { Length: > TrailValueMaxLength } ? value[..TrailValueMaxLength] : value;
}
