using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the storefront's third-party content (T224 KK9; 02 12.2.5; 10 KD-19, KD-41, KD-55): the site has no visitor analytics, no third-party
/// cookie, no tracking pixel, no advertising tag, no cookie consent banner and no embedded map, video or social feed; the one exception is the payment
/// provider's frame, whose mandatory cookies need no consent. Two scans, both over text and both writing nothing:
/// <see cref="Scan"/> reads a page, a script, a style sheet or a code file and finds (1) an analytics, pixel or advertising name, (2) a consent banner name,
/// (3) an embedded frame, object or embed element, or a map, video or social embed address, (4) a script, style sheet, image, source or font taken from
/// another origin; <see cref="ScanPackages"/> reads a package file (props, project, lock) and finds an analytics, advertising or tag package.
/// The inline frame (iframe) is the only allowed exception and only on a page of <see cref="PaymentFramePages"/>, a closed list of one view (T510: the payment
/// page's frame); there its address must be a path of the site, an address under https://www.paytr.com/ or the payment page's one checked property
/// <c>@Model.PaymentFrameAddress</c> - the layer's address, which the page model takes only under the provider's form root (<c>PaymentFrameSource.Holds</c>; the
/// application's text may not name the provider outside the layer, 08 3.1 rule 1) - (<see cref="FrameSource"/>). A frame
/// (frame, frameset), object or embed element is a finding on every page, and the other rules hold on the payment page too. Full-line comments of code files are blanked first and keep their line.
/// </summary>
internal static partial class ThirdPartyContentScan
{
    public const string TrackingName = "names an analytics, tracking pixel or advertising tag (02 12.2.5; KD-19: no visitor analytics, no third-party cookie, no tracking pixel, no advertising tag)";
    public const string ConsentBannerName = "names a cookie consent banner (02 12.2.5: the cookies are strictly necessary and need no consent, so there is no banner)";
    public const string EmbeddedContent = "embeds a frame, object, map, video or social feed (KD-41, KD-55: no embedded content; the payment provider's frame on a PaymentFramePages page is the only exception)";
    public const string FrameSource = "gives a payment frame an address other than the site's own paths or https://www.paytr.com/ (05 6.5.4: frame-src is 'self' and the provider's origin), or a srcdoc";
    public const string ExternalResource = "loads a script, style sheet, image, source or font from another origin (02 12.2.5: no third-party content)";
    public const string TrackingPackage = "references an analytics, advertising or tag package (02 12.2.5; KD-19)";

    /// <summary>
    /// The pages that may hold the payment provider's frame element, as repository-relative paths. Closed: the payment page's frame view (T510; 07 UC-29) and
    /// nothing else may hold a frame. The provider's cookies stay the cookie policy draft's placeholder until the test mode shows them (02 12.2.5; D-1467).
    /// </summary>
    public static readonly IReadOnlyCollection<string> PaymentFramePages = ["src/Shopfolio.Web/Pages/Orders/_PaymentFrame.cshtml"];

    /// <summary>The files whose text is markup: only these are searched for an element in angle brackets (D-114).</summary>
    public static readonly string[] MarkupExtensions = [".cshtml", ".razor", ".html", ".htm", ".svg"];

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text) => Scan(relativePath, text, PaymentFramePages);

    /// <summary>The scan with the list of payment frame pages given, so a test can show that the exception is one page wide and one element wide.</summary>
    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text, IReadOnlyCollection<string> framePages)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = path.EndsWith(".cs", StringComparison.Ordinal) ? BlankCommentLines(text.ReplaceLineEndings("\n")) : text.ReplaceLineEndings("\n");
        List<(string Rule, int Line)> findings = [];
        Add(findings, TrackingName, Tracking(), clean);
        Add(findings, ConsentBannerName, Banner(), clean);
        Add(findings, EmbeddedContent, EmbedAddress(), clean);

        // The element rules read markup (D-114): in a code or script file a "<object>" is a generic type argument (09 15.1 item 5 writes PasswordHasher<object>), so there only the
        // createElement form of an element is searched.
        bool isMarkup = MarkupExtensions.Any(e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase));
        Add(findings, EmbeddedContent, isMarkup ? OtherEmbedElement() : OtherCreatedElement(), clean);
        if (framePages.Contains(path, StringComparer.Ordinal))
        {
            Add(findings, FrameSource, ForeignFrameSource(), clean);
        }
        else
        {
            Add(findings, EmbeddedContent, isMarkup ? FrameElement() : FrameCreatedElement(), clean);
        }

        Add(findings, ExternalResource, ForeignResource(), clean);
        return [.. findings.OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    public static IReadOnlyList<(string Rule, int Line)> ScanPackages(string text)
    {
        List<(string Rule, int Line)> findings = [];
        Add(findings, TrackingPackage, PackageName(), text.ReplaceLineEndings("\n"));
        return findings;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            int line = text.AsSpan(0, match.Index).Count('\n') + 1;
            if (!findings.Contains((rule, line)))
            {
                findings.Add((rule, line));
            }
        }
    }

    private static string BlankCommentLines(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    // Analytics, pixel and advertising names and addresses of the common services, and their global functions.
    [GeneratedRegex(
        @"google-analytics|googletagmanager|googletagservices|googlesyndication|doubleclick|adsbygoogle|\bgtag\s*\(|\bfbq\s*\(|connect\.facebook\.net|facebook\.com/tr|hotjar|clarity\.ms|mouseflow|fullstory|plausible\.io|matomo|piwik|mixpanel|amplitude\.com|segment\.(?:com|io)|analytics\.tiktok|snap\.licdn|static\.ads-twitter|ads\.linkedin|yandex\.ru/metrika|mc\.yandex|_paq\b|\bdataLayer\b",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex Tracking();

    [GeneratedRegex(@"cookie[-_ ]?(?:banner|consent|notice|bar)|consent[-_ ]?(?:banner|bar|manager)|cookieconsent|cookiebot|onetrust|çerez[ -]onay", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Banner();

    // The addresses of embedded maps, video players and social feeds (any file, the frame exception included).
    [GeneratedRegex(
        @"google\.com/maps|maps\.googleapis|maps\.google\.|openstreetmap\.org/export|youtube\.com/embed|youtube-nocookie|player\.vimeo|platform\.twitter|platform\.instagram|instagram\.com/embed|facebook\.com/plugins|tiktok\.com/embed|embed\.spotify|w\.soundcloud",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmbedAddress();

    // The inline frame element: the one element that is allowed, and only on a payment frame page.
    [GeneratedRegex(@"<\s*iframe\b|createElement\s*\(\s*['""]iframe['""]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FrameElement();

    // The same element made in a code or script file: only the createElement form (D-114).
    [GeneratedRegex(@"createElement\s*\(\s*['""]iframe['""]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FrameCreatedElement();

    // The other elements that embed another document: frame, frameset, object, embed - a finding on every page, the payment frame pages included.
    [GeneratedRegex(@"<\s*(?:frame|frameset|object|embed)\b|createElement\s*\(\s*['""](?:frame|frameset|object|embed)['""]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OtherEmbedElement();

    [GeneratedRegex(@"createElement\s*\(\s*['""](?:frame|frameset|object|embed)['""]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OtherCreatedElement();

    // On a payment frame page the frame takes its address from the provider's origin or from the site itself (05 6.5.4: frame-src is 'self' and https://www.paytr.com):
    // a src that is neither a path of the site nor an address under https://www.paytr.com/, and a srcdoc, are findings.
    [GeneratedRegex(
        @"<\s*iframe\b[^>]*?\bsrc\s*=\s*(?>[""']?\s*)(?!https://www\.paytr\.com/|/(?!/)|@Model\.PaymentFrameAddress[""'\s>])|<\s*iframe\b[^>]*?\bsrcdoc\s*=",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex ForeignFrameSource();

    // src= or href= of an element that loads a resource (script, link, img, source, video, audio, track) and url(...) or @import in a style, taking an
    // absolute or protocol-relative address. An anchor's href is a link and not a resource, so it is not matched.
    [GeneratedRegex(
        @"<\s*(?:script|img|source|video|audio|track|input)\b[^>]*?\bsrc\s*=\s*[""']?\s*(?:https?:)?//|<\s*link\b[^>]*?\bhref\s*=\s*[""']?\s*(?:https?:)?//|\burl\s*\(\s*[""']?\s*(?:https?:)?//|@import\s+(?:url\s*\(\s*)?[""']?\s*(?:https?:)?//",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex ForeignResource();

    // A package id or a package reference naming an analytics, advertising or tag library.
    [GeneratedRegex(
        @"ApplicationInsights|Analytics|GoogleTagManager|GoogleAds|AdSense|Mixpanel|Hotjar|Matomo|Piwik|Amplitude|FacebookPixel|TikTokPixel",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex PackageName();
}
