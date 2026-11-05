using System.Text.RegularExpressions;

namespace Shopfolio.Infrastructure.Routing;

/// <summary>
/// The address rules of the site (07 2.1; K-1162): the path segments are Turkish and made of lower-case ASCII letters, digits and single hyphens; a
/// record's address is <c>/{prefix}/{Slug}</c> with one of five prefixes; the first path segment is a closed list. The list and the segment rule
/// live here and nowhere else: the page convention (<see cref="AddressConvention"/>) checks every page template against them when the application
/// starts, and the tests check every mapped endpoint against them. Adding a first segment is a change of 07 2.1.2 and of K-1162, not of this file alone.
/// </summary>
internal static partial class AddressRules
{
    /// <summary>The prefix of a product record (07 2.1.1).</summary>
    public const string ProductPrefix = "urun";

    /// <summary>The prefix of a category record.</summary>
    public const string CategoryPrefix = "kategori";

    /// <summary>The prefix of a service introduction record.</summary>
    public const string ServicePrefix = "hizmet";

    /// <summary>The prefix of a reference work record.</summary>
    public const string ReferencePrefix = "referans";

    /// <summary>The prefix of a general page record.</summary>
    public const string GeneralPagePrefix = "sayfa";

    /// <summary>The panel's path area (07 2.1.5; 05 3.5.1): the one area where an internal key may be part of the path.</summary>
    public const string PanelArea = "panel";

    /// <summary>The path the static files are served under (07 2.1.2, 3.1.6): <c>/statik/css/base.css</c>.</summary>
    public const string StaticArea = "/statik";

    /// <summary>The order root (07 2.1.3); its two roots are in <see cref="OrderRoutes"/>.</summary>
    public const string OrderArea = "siparis";

    /// <summary>The five record prefixes (07 2.1.1); there is no language prefix (KD-20; 07 2.1.1).</summary>
    public static readonly IReadOnlyList<string> RecordPrefixes = [ProductPrefix, CategoryPrefix, ServicePrefix, ReferencePrefix, GeneralPagePrefix];

    /// <summary>The fixed pages: the first segment of each of them (07 2.1.2; the home page <c>/</c> has no segment).</summary>
    public static readonly IReadOnlyList<string> FixedPages =
    [
        "arama", "hizmetler", "referanslar", "hakkimizda", "sss", "iletisim", "aydinlatma-metni", "cerez-politikasi", "islem-rehberi", "sepet", "odeme",
        "siparis-takibi",
    ];

    /// <summary>The account links and the account area (07 2.1.2).</summary>
    public static readonly IReadOnlyList<string> AccountPages = ["kayit", "giris", "cikis", "sifre-sifirlama", "dogrulama", "geri-al", "hesap"];

    /// <summary>The machine and file areas (07 2.1.2, 2.1.9).</summary>
    public static readonly IReadOnlyList<string> MachineAreas = ["sistem", "indir", "gorsel", "statik"];

    /// <summary>
    /// The two protocol files whose names are fixed by the protocol (07 2.1.9; 05 3.7.4): the only first segments that carry a dot.
    /// </summary>
    public static readonly IReadOnlyList<string> ProtocolFiles = ["sitemap.xml", "robots.txt"];

    /// <summary>The closed list of the first path segment (07 2.1.2): the five prefixes, the fixed pages, the account, the order root, the panel and the machine areas.</summary>
    public static readonly IReadOnlyList<string> ClosedFirstSegments =
    [
        .. RecordPrefixes, .. FixedPages, .. AccountPages, OrderArea, PanelArea, .. MachineAreas, .. ProtocolFiles,
    ];

    private static readonly HashSet<string> _closed = new(ClosedFirstSegments, StringComparer.Ordinal);

    /// <summary>
    /// Whether a literal path segment is a valid address segment: lower-case ASCII letters, digits and single hyphens, none at either end (07 2.1; K-1162).
    /// </summary>
    public static bool IsValidSegment(string segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        return SegmentPattern().IsMatch(segment);
    }

    /// <summary>Whether the text is a member of the closed list of first path segments (07 2.1.2); the comparison is exact, the list is lower case.</summary>
    public static bool IsClosedFirstSegment(string segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        return _closed.Contains(segment);
    }

    /// <summary>
    /// The address of a record: <c>/{prefix}/{slug}</c>, lower case and without a trailing slash (07 2.1; 2.1.7). The prefix is one of the five and the
    /// slug is a valid segment; anything else is a programming error (the slug of a record is validated when it is saved, 06 4.72).
    /// </summary>
    public static string RecordAddress(string prefix, string slug)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(slug);
        if (!RecordPrefixes.Contains(prefix, StringComparer.Ordinal))
        {
            throw new ArgumentException("The prefix is not one of the five record prefixes (07 2.1.1).", nameof(prefix));
        }

        if (!IsValidSegment(slug))
        {
            throw new ArgumentException("The slug is not a valid address segment (07 2.1).", nameof(slug));
        }

        return $"/{prefix}/{slug}";
    }

    /// <summary>
    /// The slug as the lookup compares it: the incoming path matches without regard to letter case (07 2.1.7), and the stored slug is lower case.
    /// The fold is the invariant one - the Turkish culture would turn <c>I</c> into the dotless <c>ı</c>. The values that are case sensitive
    /// (an access key, a link key, a download value) are never passed through here.
    /// </summary>
    public static string SlugForLookup(string slugFromPath)
    {
        ArgumentNullException.ThrowIfNull(slugFromPath);
        return slugFromPath.ToLowerInvariant();
    }

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SegmentPattern();
}
