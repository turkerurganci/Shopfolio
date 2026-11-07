using System.Text;
using System.Xml.Linq;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The contents of the site map and of robots.txt without a database (T313 criteria 2 to 5; KK2 to KK5; 07 4.32.1; 05 3.7.3, 3.7.4): the fixed list pages come first and
/// are open pages of the closed list, a closed path never gets in, every address is one lower-case form with the installation's domain, the XML is well formed and
/// escaped, and robots.txt is one line that names the site map and blocks nothing.
/// </summary>
public sealed class SiteMapFilesTests
{
    private static SiteAddress Site(string domain = "magaza.example.com") => new(Options.Create(new SiteOptions { Domain = domain }));

    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    [Fact]
    public void TheFixedListPages_AreFixedPagesOfTheClosedList_AndNoneOfThemIsClosedToTheSearchEngines()
    {
        Assert.Equal("/", SiteMapFiles.FixedListPaths[0]);
        foreach (string path in SiteMapFiles.FixedListPaths.Skip(1))
        {
            Assert.Contains(path.TrimStart('/'), AddressRules.FixedPages);
        }

        Assert.All(SiteMapFiles.FixedListPaths, path => Assert.False(SearchEngineClosure.IsClosedPath(path), path));
        Assert.Equal(SiteMapFiles.FixedListPaths.Count, SiteMapFiles.FixedListPaths.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Compose_PutsTheFixedPagesFirst_ThenTheRecordsInPathOrder_EachOnce()
    {
        IReadOnlyList<string> paths = SiteMapFiles.Compose(
            ["/urun/b-masa", "/kategori/mobilya", "/urun/a-sandalye", "/urun/b-masa"],
            ["/sayfa/kvkk", "/hizmet/montaj", "/hizmetler", "/referans/ofis", "/hakkimizda"]);

        Assert.Equal(
            [
                .. SiteMapFiles.FixedListPaths,
                "/hakkimizda",
                "/hizmet/montaj",
                "/kategori/mobilya",
                "/referans/ofis",
                "/sayfa/kvkk",
                "/urun/a-sandalye",
                "/urun/b-masa",
            ],
            paths);
    }

    [Theory]
    [InlineData("/arama")]
    [InlineData("/arama/sonuc")]
    [InlineData("/sepet")]
    [InlineData("/odeme")]
    [InlineData("/odeme/donus/x")]
    [InlineData("/siparis-takibi")]
    [InlineData("/siparis/erisim/abc")]
    [InlineData("/giris")]
    [InlineData("/hesap/siparisler")]
    [InlineData("/kayit")]
    [InlineData("/panel/urunler")]
    [InlineData("/indir/abc")]
    [InlineData("/PANEL/urunler")]
    public void Compose_LeavesOutAClosedPath(string closed)
    {
        IReadOnlyList<string> paths = SiteMapFiles.Compose([closed, "/urun/masa"], []);

        Assert.DoesNotContain(paths, p => string.Equals(p, closed, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("/urun/masa", paths);
    }

    [Fact]
    public void Compose_MakesEveryAddressOneLowerCaseForm_AndDropsWhatIsNotAPagePath()
    {
        IReadOnlyList<string> paths = SiteMapFiles.Compose(
            ["/Urun/Ahsap-Masa/", "/urun/masa?page=2", "/urun/masa#x", "urun/masa", "", "//evil.example.com/x", "/urun/a b", "/urun\\x", "///"],
            ["/SAYFA/Hakkinda"]);

        string[] records = [.. paths.Skip(SiteMapFiles.FixedListPaths.Count)];
        Assert.Equal(["/sayfa/hakkinda", "/urun/ahsap-masa"], records);
        Assert.All(paths, p => Assert.Equal(p.ToLowerInvariant(), p));
    }

    [Fact]
    public void Xml_IsWellFormed_HasOneLocPerPathWithTheInstallationDomain_AndNoRequestHost()
    {
        string xml = SiteMapFiles.Xml(Site(), ["/", "/urun/masa", "/kategori/oturma-odasi"]);

        XDocument document = XDocument.Parse(xml);
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n", xml, StringComparison.Ordinal);
        Assert.Equal(Ns + "urlset", document.Root?.Name);
        Assert.Equal(
            ["https://magaza.example.com/", "https://magaza.example.com/urun/masa", "https://magaza.example.com/kategori/oturma-odasi"],
            document.Root!.Elements(Ns + "url").Select(u => u.Element(Ns + "loc")?.Value));
        Assert.All(document.Root.Elements(Ns + "url"), u => Assert.Single(u.Elements()));
    }

    [Fact]
    public void Xml_EscapesWhatXmlReserves_AndAnEmptyMapIsStillAWellFormedDocument()
    {
        // A slug cannot hold these, but the writer owns the escaping: the text is not pasted together by hand.
        string xml = SiteMapFiles.Xml(Site(), ["/urun/a&b<c>"]);

        Assert.Contains("&amp;", xml, StringComparison.Ordinal);
        Assert.DoesNotContain("a&b", xml, StringComparison.Ordinal);
        Assert.Equal("https://magaza.example.com/urun/a&b<c>", XDocument.Parse(xml).Descendants(Ns + "loc").Single().Value);
        Assert.Empty(XDocument.Parse(SiteMapFiles.Xml(Site(), [])).Root!.Elements());
    }

    [Fact]
    public void Xml_RefusesAPathThatIsNotAPathOfTheSite()
    {
        Assert.Throws<ArgumentException>(() => SiteMapFiles.Xml(Site(), ["https://evil.example.com/x"]));
        Assert.Throws<ArgumentException>(() => SiteMapFiles.Xml(Site(), ["//evil.example.com/x"]));
    }

    [Fact]
    public void Robots_IsOneLineWithTheSiteMapAddressOfTheInstallation_AndHasNoRuleThatBlocksAPage()
    {
        string robots = SiteMapFiles.Robots(Site("shop.example.org"));

        Assert.Equal("Sitemap: https://shop.example.org/sitemap.xml\n", robots);
        Assert.Single(robots.Split('\n', StringSplitOptions.RemoveEmptyEntries));
        Assert.DoesNotContain("disallow", robots, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("user-agent", robots, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("noindex", robots, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(robots, Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(robots)));
    }
}
