using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the storefront's third-party content (T224 KK9; 02 12.2.5; 10 KD-19, KD-41, KD-55): the site has no visitor analytics, no third-party
/// cookie, no tracking pixel, no advertising tag, no cookie consent banner and no embedded map, video or social feed; the one exception is the payment
/// provider's frame, whose mandatory cookies need no consent. Two scans, both over text and both writing nothing:
/// <see cref="Scan"/> reads a page, a script, a style sheet or a code file and finds (1) an analytics, pixel or advertising name, (2) a consent banner name,
/// (3) an embedded frame, object or embed element, or a map, video or social embed address, (4) a script, style sheet, image, source or font taken from
/// another origin; <see cref="ScanPackages"/> reads a package file (props, project, lock) and finds an analytics, advertising or tag package.
/// The frame is the only allowed exception and only on a page of <see cref="PaymentFramePages"/>, a closed list that is empty until the payment page
/// task adds its page; the frame element alone is allowed there, the other rules hold on it. Full-line comments of code files are blanked first and keep their line.
/// </summary>
internal static partial class ThirdPartyContentScan
{
    public const string TrackingName = "names an analytics, tracking pixel or advertising tag (02 12.2.5; KD-19: no visitor analytics, no third-party cookie, no tracking pixel, no advertising tag)";
    public const string ConsentBannerName = "names a cookie consent banner (02 12.2.5: the cookies are strictly necessary and need no consent, so there is no banner)";
    public const string EmbeddedContent = "embeds a frame, object, map, video or social feed (KD-41, KD-55: no embedded content; the payment provider's frame on a PaymentFramePages page is the only exception)";
    public const string ExternalResource = "loads a script, style sheet, image, source or font from another origin (02 12.2.5: no third-party content)";
    public const string TrackingPackage = "references an analytics, advertising or tag package (02 12.2.5; KD-19)";

    /// <summary>
    /// The pages that may hold the payment provider's frame element, as repository-relative paths. Closed and empty: the payment page task adds its page here
    /// (and the provider's cookies to the cookie policy draft placeholder, 02 12.2.5); nothing else may hold a frame.
    /// </summary>
    public static readonly IReadOnlyCollection<string> PaymentFramePages = [];

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
        if (!framePages.Contains(path, StringComparer.Ordinal))
        {
            Add(findings, EmbeddedContent, EmbedElement(), clean);
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

    // The elements that embed another document: frame, object, embed (a frame element alone is allowed on a payment frame page).
    [GeneratedRegex(@"<\s*(?:iframe|frame|frameset|object|embed)\b|createElement\s*\(\s*['""](?:iframe|object|embed)['""]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmbedElement();

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
