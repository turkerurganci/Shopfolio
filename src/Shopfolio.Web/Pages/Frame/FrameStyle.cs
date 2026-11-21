using System.Text.RegularExpressions;

using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// The values the layouts write that are not markup (T540): the one inline style block that carries the brand colour (05 3.8.3; 09 16.1 item 6), the address of the brand's images, the
/// WhatsApp link and the names of the social platforms. Written here once; a layout calls and never builds a string of its own.
/// </summary>
public static partial class FrameStyle
{
    /// <summary>The product's default brand colour and the white on it, used only if a value that reaches the layout is not a six-digit colour (the settings validate it; this is the last line, 02 3.29.4).</summary>
    internal const string DefaultBrandColor = "#1A56DB";

    internal const string DefaultBrandTextColor = "#FFFFFF";

    /// <summary>The path prefix of the brand's images (07 2.1.2: <c>/gorsel</c>).</summary>
    internal const string ImagePrefix = "/gorsel/";

    /// <summary>The WhatsApp chat address of a ten-digit national number: <c>wa.me</c> with the Turkish country code (02 3.32.9; 04 2.2.3). The number is digits only, so nothing can leave the address.</summary>
    internal const string WhatsAppPrefix = "https://wa.me/90";

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ColorPattern();

    [GeneratedRegex("^[0-9]{10}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TenDigitsPattern();

    /// <summary>
    /// The content of the page head's style block: the two variables the style sheets read (<c>--brand-color</c>, <c>--brand-text-color</c>). A value that is not a six-digit colour is replaced by the
    /// product's default, so the block can never carry anything but two colours - the layout writes it with the response's nonce and no style attribute exists anywhere (09 15.2 item 4).
    /// </summary>
    public static string BrandVariables(FrameBrand brand)
    {
        ArgumentNullException.ThrowIfNull(brand);
        bool valid = ColorPattern().IsMatch(brand.BrandColor) && ColorPattern().IsMatch(brand.BrandTextColor);
        string color = valid ? brand.BrandColor : DefaultBrandColor;
        string text = valid ? brand.BrandTextColor : DefaultBrandTextColor;
        return $":root{{--brand-color:{color};--brand-text-color:{text};}}";
    }

    /// <summary>The address of the fallback site icon, served while no site icon is uploaded (02 3.29.4; D-1135): inside the image area, so no new first segment.</summary>
    public const string SiteIconPath = ImagePrefix + "simge";

    /// <summary>The media type of the fallback site icon.</summary>
    public const string SiteIconContentType = "image/svg+xml";

    /// <summary>
    /// The fallback site icon: the brand name's first letter on the brand colour (02 3.29.4). Both colours are checked like <see cref="BrandVariables"/> does and the letter is escaped, so the
    /// image can carry nothing but a letter and two colours.
    /// </summary>
    public static string FallbackIconSvg(string initial, string brandColor, string brandTextColor)
    {
        ArgumentNullException.ThrowIfNull(initial);
        ArgumentNullException.ThrowIfNull(brandColor);
        ArgumentNullException.ThrowIfNull(brandTextColor);
        bool valid = ColorPattern().IsMatch(brandColor) && ColorPattern().IsMatch(brandTextColor);
        string color = valid ? brandColor : DefaultBrandColor;
        string text = valid ? brandTextColor : DefaultBrandTextColor;
        string letter = System.Net.WebUtility.HtmlEncode(initial);
        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"64\" height=\"64\" viewBox=\"0 0 64 64\"><rect width=\"64\" height=\"64\" rx=\"12\" fill=\"{color}\"/>"
            + $"<text x=\"32\" y=\"43\" text-anchor=\"middle\" font-family=\"sans-serif\" font-size=\"32\" font-weight=\"700\" fill=\"{text}\">{letter}</text></svg>";
    }

    /// <summary>The address of a brand image - the logo or the site icon - by its file id (07 2.1.2).</summary>
    public static string ImagePath(Guid fileId) => ImagePrefix + fileId.ToString("D");

    /// <summary>The WhatsApp chat link of the firm's number, or null when the value is not ten digits (06 4.2; the setting validates it).</summary>
    public static string? WhatsAppLink(string? number) => number is not null && TenDigitsPattern().IsMatch(number) ? WhatsAppPrefix + number : null;

    /// <summary>The name a social platform is shown with.</summary>
    public static string SocialName(SocialPlatform platform) => Enum.IsDefined(platform)
        ? platform.ToString()
        : throw new ArgumentOutOfRangeException(nameof(platform), platform, "A social platform of the closed list (06 4.55).");
}
