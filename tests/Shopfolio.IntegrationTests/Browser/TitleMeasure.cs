namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The measure of a screen's title in the inventory walk (D-1710): a title is the screen's own when it is the product name alone or ends with " - " and the product
/// name - the brand name of the seed on the storefront (<c>_StorefrontLayout</c>), the panel's product title on the panel (<c>_PanelLayout</c>,
/// <c>_PanelBareLayout</c>). The walk used to ask only for a title that is not empty, and a page that was given no brand name was titled with the bare host name
/// ("localhost"), which passed; with this measure that title fails.
/// </summary>
internal static class TitleMeasure
{
    public const string PanelName = "Shopfolio Panel";

    private const string PanelPathPrefix = "/panel";

    /// <summary>
    /// The two screens whose title leaves the brand name out today: the FAQ page and the contact page hand the search engine head the page's name alone
    /// (<c>StorefrontHeads.Faq</c> and <c>.Contact</c> say "the page's name with the brand name as the title"; the head writes the name only) - a product defect
    /// (src; D-1788, T604 U). Known and exact: the address and the title it has; a page that starts carrying the brand name fails the walk until the entry is removed.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> KnownWithoutBrand = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["/sss"] = "Sık sorulan sorular",
        ["/iletisim"] = "İletişim",
    };

    /// <summary>The product name the title of the path must carry: the panel's for an address under the panel, the brand name otherwise.</summary>
    public static string ExpectedName(string path, string brandName)
    {
        bool panel = path.Equals(PanelPathPrefix, StringComparison.Ordinal)
            || path.StartsWith(PanelPathPrefix + "/", StringComparison.Ordinal)
            || path.StartsWith(PanelPathPrefix + "?", StringComparison.Ordinal);
        return panel ? PanelName : brandName;
    }

    /// <summary>
    /// True when the title is the name alone or "&lt;screen&gt; - &lt;name&gt;" (a non-empty screen part); the separator is the frame's hyphen or the en dash of the search engine
    /// title (<c>SearchText.TitleSeparator</c>, 02 3.30.5 item 1). A title that only contains the name elsewhere, or is the host name, is not.
    /// </summary>
    public static bool Names(string title, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return title.Equals(name, StringComparison.Ordinal) || Tail(title, name, " - ") || Tail(title, name, " – ");
    }

    private static bool Tail(string title, string name, string separator)
    {
        string suffix = separator + name;
        return title.EndsWith(suffix, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(title[..^suffix.Length]);
    }
}

public sealed class TitleMeasureTests
{
    [Theory]
    [InlineData("/panel", "Shopfolio Panel")]
    [InlineData("/panel/siparisler", "Shopfolio Panel")]
    [InlineData("/panel?x=1", "Shopfolio Panel")]
    [InlineData("/panelim", "Örnek Mağaza")]
    [InlineData("/", "Örnek Mağaza")]
    [InlineData("/urun/yok", "Örnek Mağaza")]
    public void ExpectedName_IsThePanelsUnderThePanel_AndTheBrandElsewhere_D1710(string path, string expected)
    {
        Assert.Equal(expected, TitleMeasure.ExpectedName(path, "Örnek Mağaza"));
    }

    [Theory]
    [InlineData("Örnek Mağaza", true)]
    [InlineData("Sepet - Örnek Mağaza", true)]
    [InlineData("Mobilya – Örnek Mağaza", true)]
    [InlineData("Mobilya — Örnek Mağaza", false)]
    [InlineData("Ürün - Alt başlık - Örnek Mağaza", true)]
    [InlineData("localhost", false)]
    [InlineData("", false)]
    [InlineData(" - Örnek Mağaza", false)]
    [InlineData("Örnek Mağaza - Sepet", false)]
    [InlineData("Sepet - Örnek Mağazalar", false)]
    [InlineData("örnek mağaza", false)]
    public void Names_AcceptsTheNameAloneOrAsTheTail_AndNotTheHostName_D1710(string title, bool expected)
    {
        Assert.Equal(expected, TitleMeasure.Names(title, "Örnek Mağaza"));
    }

    [Fact]
    public void Names_WithNoName_IsNeverTrue_D1710()
    {
        Assert.False(TitleMeasure.Names("localhost", string.Empty));
        Assert.False(TitleMeasure.Names(string.Empty, string.Empty));
    }
}
