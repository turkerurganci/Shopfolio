using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the closed list of the first path segments and the order roots of the code equal to <c>07 2.1</c> in both directions (T206 criteria 2 and 3;
/// KK2, KK3; 09 21.4 item 1): a segment added to the code without the document, or listed in the document without a constant, turns this red; the
/// access-key root stays the keyed path the log masking knows. Reads repository files only. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class AddressListConsistencyTests
{
    private const string PrefixesStart = "**2.1.1 Önekler**";

    private const string ListStart = "**2.1.2 İlk yol bölümü kapalı bir listedir:**";

    /// <summary>The sentence after the list in 07 2.1.2; the text before it is the list.</summary>
    private const string ListEnd = "Kayıt adresi her zaman iki bölümlüdür";

    private static string[] Paragraphs()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "07_API_DESIGN.md")).ReplaceLineEndings("\n");
        return text.Split('\n');
    }

    private static string Paragraph(string[] lines, string start) =>
        Assert.Single(lines, l => l.StartsWith(start, StringComparison.Ordinal));

    [Fact]
    public void FirstSegmentsOf07_EqualTheClosedListOfTheCode_InBothDirections()
    {
        string[] lines = Paragraphs();
        string prefixes = Paragraph(lines, PrefixesStart);
        string list = Paragraph(lines, ListStart);
        string listOnly = list[..list.IndexOf(ListEnd, StringComparison.Ordinal)];

        string[] documented =
        [
            .. FirstSegments(prefixes),
            .. FirstSegments(listOnly),
        ];

        Assert.NotEmpty(documented);
        Assert.Empty(SetComparison.Compare("07 2.1.2 first segments", documented.Distinct(StringComparer.Ordinal), AddressRules.ClosedFirstSegments));
        Assert.Equal(AddressRules.RecordPrefixes.Order(StringComparer.Ordinal), FirstSegments(prefixes).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheFivePrefixesOf07_HaveTheirExampleAddress()
    {
        string prefixes = Paragraph(Paragraphs(), PrefixesStart);

        Assert.Contains($"`{AddressRules.RecordAddress(AddressRules.ProductPrefix, "ahsap-masa")}`", prefixes, StringComparison.Ordinal);
    }

    [Fact]
    public void TheOrderRootsOf07_AreTheOnesOfTheCode()
    {
        string[] lines = Paragraphs();
        string roots = Paragraph(lines, "**2.1.3 Sipariş sayfası iki kökten açılır:**");

        Assert.Contains("`/siparis/{sipariş numarası}`", roots, StringComparison.Ordinal);
        Assert.Contains("`/siparis/erisim/{erişim anahtarı}`", roots, StringComparison.Ordinal);
        string[] screens = [.. ScreenNames().Matches(roots).Select(m => m.Groups[1].Value)];
        Assert.Empty(SetComparison.Compare("07 2.1.3 order screens", screens, OrderRoutes.Screens));
        Assert.StartsWith("/siparis/", OrderRoutes.MemberRoot, StringComparison.Ordinal);
        Assert.StartsWith("/siparis/" + OrderRoutes.AccessSegment + "/", OrderRoutes.AccessRoot, StringComparison.Ordinal);
    }

    [Fact]
    public void TheAccessKeyRoot_IsAKeyedPathOfTheLogMasking()
    {
        // The masking replaces the segment after /siparis/erisim (09 18.2 item 2b); the access root has the key right there.
        Assert.Contains(PathMasker.KeyedPrefixes, p => p.SequenceEqual([AddressRules.OrderArea, OrderRoutes.AccessSegment]));
        Assert.StartsWith($"/{AddressRules.OrderArea}/{OrderRoutes.AccessSegment}/{{", OrderRoutes.AccessRoot, StringComparison.Ordinal);
    }

    private static List<string> FirstSegments(string text)
    {
        List<string> segments = [];
        foreach (Match match in PathInBackticks().Matches(text))
        {
            string first = match.Groups[1].Value;
            if (first.Length > 0)
            {
                segments.Add(first);
            }
        }

        return segments;
    }

    // A path in backticks that starts with a slash: the first segment is what follows it up to the next slash or the closing tick.
    [GeneratedRegex(@"`/([^/`…]*)[^`]*`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PathInBackticks();

    // The screens written as `…/teyit` in 07 2.1.3.
    [GeneratedRegex(@"`…/([a-z-]+)`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScreenNames();
}
