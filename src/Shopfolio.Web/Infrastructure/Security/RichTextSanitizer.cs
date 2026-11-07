using AngleSharp.Css.Dom;
using AngleSharp.Dom;

using Ganss.Xss;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one place of the closed text format set's cleaning (09 15.3 items 1-5; 06 2.10; 02 3.11.6): the allow list is written out here and the library is built from
/// it alone, never from the library's own default set. The set is five formats - bold (<c>strong</c>), italic (<c>em</c>), the bullet list (<c>ul</c>, <c>li</c>), the link
/// (<c>a</c>) and the single-level subheading (<c>h3</c>) - and the two carriers of the text, the paragraph (<c>p</c>) and the line break (<c>br</c>), which are not
/// formats of the set (K-1318). The one attribute is <c>href</c> on <c>a</c>; the schemes are <c>http</c>, <c>https</c> and <c>mailto</c>. Class, style, event
/// attributes and <c>target</c> do not exist. A format outside the set falls and its text stays; an element that carries code or other documents falls with its content.
/// The list only grows (06 2.10.2): the display filters the stored text again through the same list, so a narrowing would change a published text.
/// </summary>
internal static class RichTextSanitizer
{
    /// <summary>The allowed tags: the five formats and the two carriers.</summary>
    public static readonly IReadOnlySet<string> AllowedTags = new HashSet<string>(StringComparer.Ordinal)
    {
        "strong", "em", "ul", "li", "a", "h3", "p", "br",
    };

    /// <summary>The allowed attributes: <c>href</c>, and only on <c>a</c> (the rest is enforced after the cleaning).</summary>
    public static readonly IReadOnlySet<string> AllowedAttributes = new HashSet<string>(StringComparer.Ordinal) { "href" };

    /// <summary>The allowed link schemes: an absolute address only.</summary>
    public static readonly IReadOnlySet<string> AllowedSchemes = new HashSet<string>(StringComparer.Ordinal) { "http", "https", "mailto" };

    /// <summary>The elements that fall together with what they contain: code, styles, embedded documents and the two foreign-content roots.</summary>
    private static readonly HashSet<string> DroppedWithContent = new HashSet<string>(StringComparer.Ordinal)
    {
        "script", "style", "iframe", "frame", "frameset", "object", "embed", "applet", "noscript", "noembed", "noframes", "template",
        "svg", "math", "title", "head", "textarea", "select", "option", "xmp", "plaintext", "canvas", "audio", "video",
    };

    /// <summary>The library's cleaner is not documented as safe for two threads at once; every thread builds its own from the same list.</summary>
    private static readonly ThreadLocal<HtmlSanitizer> Cleaner = new(Build);

    /// <summary>
    /// The text as it is stored (09 15.3 item 4): the cleaned markup; null, empty and white space only are the empty text. Cleaning the cleaned text again gives the
    /// same text. Written once, at the write: the function that sets the field calls it and nothing else does.
    /// </summary>
    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        string cleaned = Cleaner.Value!.Sanitize(html);
        return string.IsNullOrWhiteSpace(cleaned) ? string.Empty : cleaned;
    }

    private static HtmlSanitizer Build()
    {
        HtmlSanitizerOptions options = new()
        {
            AllowedTags = new HashSet<string>(AllowedTags, StringComparer.OrdinalIgnoreCase),
            AllowedAttributes = new HashSet<string>(AllowedAttributes, StringComparer.OrdinalIgnoreCase),
            AllowedSchemes = new HashSet<string>(AllowedSchemes, StringComparer.OrdinalIgnoreCase),
            AllowedCssProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            AllowedAtRules = new HashSet<CssRuleType>(),
            AllowedClasses = new HashSet<string>(StringComparer.Ordinal),
            UriAttributes = new HashSet<string>(["href"], StringComparer.OrdinalIgnoreCase),
        };
        HtmlSanitizer sanitizer = new(options)
        {
            KeepChildNodes = true,
            AllowDataAttributes = false,
        };
        sanitizer.RemovingTag += (_, e) =>
        {
            if (DroppedWithContent.Contains(e.Tag.LocalName))
            {
                e.Tag.InnerHtml = string.Empty;
            }
        };

        // Every link address goes through here (09 15.3 item 3): an absolute http, https or mailto address stays; a relative one, one without a scheme and a network
        // path (//host) do not parse as an absolute address of those schemes and the attribute falls.
        sanitizer.FilterUrl += (_, e) =>
        {
            e.SanitizedUrl = Uri.TryCreate(e.OriginalUrl, UriKind.Absolute, out Uri? uri) && AllowedSchemes.Contains(uri.Scheme) && !e.OriginalUrl.StartsWith("//", StringComparison.Ordinal)
                ? e.SanitizedUrl
                : null;
        };
        sanitizer.PostProcessNode += (_, e) =>
        {
            if (e.Node is not IElement element)
            {
                return;
            }

            if (string.Equals(element.LocalName, "a", StringComparison.Ordinal))
            {
                element.SetAttribute("rel", "noopener noreferrer");
            }
            else
            {
                element.RemoveAttribute("href");
            }
        };
        return sanitizer;
    }
}
