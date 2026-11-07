using System.Text.RegularExpressions;

using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The carrier list and the tracking link patterns (T310 criteria 1-9; KK1-KK9; 08 2.4, 2.2.6 B-5; 06 5.3.12, 8.4.1; 04 5.16.8). The list is code, the link is
/// built from the number alone, and the product never calls a carrier.
/// </summary>
public sealed partial class CarrierCatalogTests
{
    private static readonly CarrierCatalog Sample = new(
        [new CarrierEntry("TestKargo", "Test Kargo", "https://track.example.com/t?no={takipNumarasi}")],
        [new RetiredCarrierEntry("EskiKargo", "Eski Kargo")]);

    [GeneratedRegex(@"^\| `(?<code>[A-Za-z]+)` \| [^|]+ \| `(?<pattern>https://[^`]+)` \|", RegexOptions.Multiline)]
    private static partial Regex DocRow();

    [Fact]
    public void Default_HoldsEveryCodeOnceWithAsciiCapitalWordAndOnePlaceholderHttpsPattern()
    {
        Assert.NotEmpty(CarrierCatalog.Default.Active);
        Assert.Equal(CarrierCatalog.Default.Active.Count, CarrierCatalog.Default.Active.Select(e => e.Code).Distinct(StringComparer.Ordinal).Count());
        foreach (CarrierEntry entry in CarrierCatalog.Default.Active)
        {
            Assert.Matches("^[A-Z][A-Za-z0-9]*$", entry.Code);
            Assert.StartsWith("https://", entry.TrackingUrlPattern, StringComparison.Ordinal);
            Assert.Equal(1, Regex.Count(entry.TrackingUrlPattern, Regex.Escape(CarrierCatalog.Placeholder)));
        }
    }

    [Fact]
    public void Default_EqualsTheTableOfTheIntegrationSpecInBothDirections()
    {
        string spec = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "08_INTEGRATION_SPEC.md"));
        Dictionary<string, string> documented = DocRow().Matches(spec).ToDictionary(m => m.Groups["code"].Value, m => m.Groups["pattern"].Value, StringComparer.Ordinal);

        Assert.Equal(
            CarrierCatalog.Default.Active.ToDictionary(e => e.Code, e => e.TrackingUrlPattern, StringComparer.Ordinal).OrderBy(p => p.Key, StringComparer.Ordinal),
            documented.OrderBy(p => p.Key, StringComparer.Ordinal));
    }

    [Fact]
    public void Default_ListIsInCodeOnly_NoSettingOrTableNamesIt()
    {
        string root = RepositoryRoot.Find();
        string[] files = Directory.EnumerateFiles(Path.Combine(root, "src", "Shopfolio.Web"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        // No DbSet, entity or setting key of the list exists: only the Order text columns hold a code name.
        Assert.DoesNotContain(files, f => File.ReadAllText(f).Contains("DbSet<CarrierEntry>", StringComparison.Ordinal));
        Assert.DoesNotContain(files, f => File.ReadAllText(f).Contains("DbSet<Carrier>", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("yurtici")]
    [InlineData("Yurtiçi")]
    [InlineData("Yurtici Kargo")]
    [InlineData("1Kargo")]
    [InlineData("Kargo-1")]
    public void Constructor_RejectsACodeThatIsNotOneAsciiCapitalWord(string code)
    {
        Assert.Throws<ArgumentException>(() => new CarrierCatalog([new CarrierEntry(code, "X", "https://x.example/{takipNumarasi}")], []));
    }

    [Fact]
    public void Constructor_RejectsACodeGivenTwice_AlsoAcrossListedAndRetired()
    {
        CarrierEntry one = new("Ayni", "A", "https://a.example/{takipNumarasi}");

        Assert.Throws<ArgumentException>(() => new CarrierCatalog([one, one], []));
        Assert.Throws<ArgumentException>(() => new CarrierCatalog([one], [new RetiredCarrierEntry("Ayni", "B")]));
    }

    [Theory]
    [InlineData("http://x.example/{takipNumarasi}")]
    [InlineData("https://x.example/")]
    [InlineData("https://x.example/{takipNumarasi}/{takipNumarasi}")]
    [InlineData("//x.example/{takipNumarasi}")]
    [InlineData("HTTPS://x.example/{takipNumarasi}")]
    public void Constructor_RejectsAPatternThatIsNotHttpsWithExactlyOnePlaceholder(string pattern)
    {
        Assert.Throws<ArgumentException>(() => new CarrierCatalog([new CarrierEntry("Kargo", "K", pattern)], []));
    }

    [Fact]
    public void BuildLink_EncodesTheNumberAndDerivesNothingElse()
    {
        TrackingLink? link = Sample.BuildLink("TestKargo", "AB 12&x=1/é#");

        Assert.NotNull(link);
        Assert.Equal("https://track.example.com/t?no=AB%2012%26x%3D1%2F%C3%A9%23", link.Href);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildLink_WithoutANumber_IsNull(string? number)
    {
        Assert.Null(Sample.BuildLink("TestKargo", number));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("EskiKargo")]
    [InlineData("Bilinmeyen")]
    public void BuildLink_ForNoCompanyARetiredOrAnUnknownOne_IsNull(string? code)
    {
        Assert.Null(Sample.BuildLink(code, "123"));
    }

    [Fact]
    public void Link_OpensInANewTabAndCarriesNoreferrer()
    {
        Assert.Equal("_blank", TrackingLink.Target);
        Assert.Contains("noreferrer", TrackingLink.Rel, StringComparison.Ordinal);
    }

    [Fact]
    public void IsSelectable_OnlyForAListedCompany()
    {
        Assert.True(Sample.IsSelectable("TestKargo"));
        Assert.False(Sample.IsSelectable("EskiKargo"));
        Assert.False(Sample.IsSelectable("Bilinmeyen"));
        Assert.False(Sample.IsSelectable(null));
    }

    [Fact]
    public void Describe_ListedCompanyWithNumber_ShowsNameNumberAndLink()
    {
        ShippingDisplay display = Sample.Describe("TestKargo", null, "123", false);

        Assert.Equal("Test Kargo", display.CarrierName);
        Assert.Equal("123", display.TrackingNumber);
        Assert.False(display.IsOwnVehicleDeclaration);
        Assert.Equal("https://track.example.com/t?no=123", display.Link?.Href);
    }

    [Fact]
    public void Describe_ListedCompanyWithoutNumber_HasNoLink()
    {
        ShippingDisplay display = Sample.Describe("TestKargo", null, null, false);

        Assert.Equal("Test Kargo", display.CarrierName);
        Assert.Null(display.TrackingNumber);
        Assert.Null(display.Link);
    }

    [Fact]
    public void Describe_Other_ShowsNameAndNumberWithoutLink()
    {
        ShippingDisplay display = Sample.Describe(null, "Yerel Nakliyat", "A-1", false);

        Assert.Equal("Yerel Nakliyat", display.CarrierName);
        Assert.Equal("A-1", display.TrackingNumber);
        Assert.Null(display.Link);
    }

    [Fact]
    public void Describe_RetiredCompany_ResolvesByNameShowsNumberWithoutLink()
    {
        ShippingDisplay display = Sample.Describe("EskiKargo", null, "A-1", false);

        Assert.Equal("Eski Kargo", display.CarrierName);
        Assert.Equal("A-1", display.TrackingNumber);
        Assert.Null(display.Link);
    }

    [Fact]
    public void Describe_OwnVehicle_ShowsTheDeclarationOnly_EvenWithStrayValues()
    {
        ShippingDisplay display = Sample.Describe("TestKargo", "Diğer", "123", true);

        Assert.True(display.IsOwnVehicleDeclaration);
        Assert.Null(display.CarrierName);
        Assert.Null(display.TrackingNumber);
        Assert.Null(display.Link);
    }

    [Fact]
    public void Source_MakesNoOutgoingRequest()
    {
        string source = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web", "Modules", "Ordering", "Contracts", "CarrierCatalog.cs"));

        foreach (string forbidden in new[] { "HttpClient", "System.Net", "WebRequest", "IHttpClientFactory" })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
        }
    }
}
