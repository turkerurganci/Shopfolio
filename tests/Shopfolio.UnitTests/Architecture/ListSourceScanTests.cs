using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The rules of the list components on the source (T545 KK1, KK5, KK6, KK9; 04 2.11, 2.13; 09 16.1): the cards are one link with no form, button, input or stock text; the list views have no sort control; the
/// panel table has no selection column and no bulk action; the grid is two, three and four columns on the three classes; the card name is the only line-clamped text; no style scrolls the panel table sideways or hides
/// one of its cells; nothing loads a list by scrolling. Each rule goes red on a deliberate violation (a text below) and stays quiet on the near-miss that is allowed.
/// </summary>
public sealed partial class ListSourceScanTests
{
    private const string Web = "src/Shopfolio.Web";

    private static readonly string[] ListViews =
    [
        "_ProductCard", "_ContentCard", "_ProductGrid", "_ContentGrid", "_Pager", "_EmptyState", "_ListSearch", "_PanelTable",
    ];

    private static readonly string[] CardViews = ["_ProductCard", "_ContentCard"];

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorComment();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    [GeneratedRegex(@"<(?:form|button|input|select|textarea)\b|\btype\s*=\s*""?checkbox", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Control();

    [GeneratedRegex(@"<select\b|\bname\s*=\s*""?(?:sort|order|orderby|siralama|sirala)\b|\b(?:sort|orderBy|siralama)\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SortControl();

    [GeneratedRegex(@"sepete\s+ekle|add\s*to\s*cart|\bstok\b|\bstock\b|\badet\b|\bpuan\b|\byorum\b|\brating\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SellingOrCounting();

    [GeneratedRegex(@"(?:@media\s*\(min-width:\s*(\d+)px\)\s*\{\s*)?\.card-grid\s*\{[^}]*grid-template-columns:\s*repeat\((\d+),", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex GridColumnRule();

    [GeneratedRegex(@"(?:-webkit-)?line-clamp\s*:", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineClamp();

    [GeneratedRegex(@"overflow(?:-x)?\s*:\s*(?:auto|scroll)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SidewaysScroll();

    [GeneratedRegex(@"IntersectionObserver|addEventListener\s*\(\s*['""]scroll['""]|onscroll|load-more|infinite|loadMore", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScrollLoading();

    private static string ReadWeb(string relative) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), Web, relative));

    private static string View(string name) => RazorComment().Replace(ReadWeb($"Pages/Shared/{name}.cshtml"), string.Empty);

    private static string Css(string file) => BlockComment().Replace(ReadWeb($"wwwroot/css/{file}"), string.Empty);

    // ---- KK1, KK4: a card is one link (04 2.11.1, 2.11.4; K-774) ----

    private static List<string> CardViolations(string view)
    {
        List<string> found =
        [
            .. Control().Matches(view).Select(m => $"control: {m.Value}"),
            .. SellingOrCounting().Matches(view).Select(m => $"selling or counting text: {m.Value}"),
        ];
        if (Regex.Count(view, @"<a\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)) > 1)
        {
            found.Add("more than one link");
        }

        return found;
    }

    [Fact]
    public void TheCardViews_AreOneLink_NoFormNoButtonNoInputNoStockOrCartText_KK1()
    {
        foreach (string name in CardViews)
        {
            Assert.Empty(CardViolations(View(name)));
        }
    }

    [Theory]
    [InlineData("<a href=\"/x\">x</a><form method=\"post\"><button>Sepete ekle</button></form>")]
    [InlineData("<a href=\"/x\">x</a><span>Stok: 3</span>")]
    [InlineData("<a href=\"/x\">x</a><input type=\"number\" name=\"quantity\" />")]
    [InlineData("<a href=\"/x\">x</a><a href=\"/y\">y</a>")]
    [InlineData("<a href=\"/x\">x</a><span>4 yorum</span>")]
    public void ACardWithACartAStockCountARatingOrASecondAction_IsFound_KK1(string view) => Assert.NotEmpty(CardViolations(view));

    [Theory]
    [InlineData("<a href=\"/x\"><img alt=\"Ürün görseli\" /><span>Tükendi</span></a>")]
    public void ACardWithTheSoldOutMarkAndOneLink_IsLeftAlone_KK1(string view) => Assert.Empty(CardViolations(view));

    // ---- KK6: no sort control (04 2.13.1) ----

    [Fact]
    public void TheListViews_HaveNoSortControl_KK6()
    {
        foreach (string name in ListViews)
        {
            Assert.Empty(SortControl().Matches(View(name)).Select(m => m.Value));
        }
    }

    [Theory]
    [InlineData("<select name=\"sort\"><option>x</option></select>")]
    [InlineData("<input name=\"orderBy\" />")]
    [InlineData("<a href=\"?sort=price\">Fiyata göre</a>")]
    [InlineData("<a href=\"?siralama=ucuz\">x</a>")]
    public void ASortControl_IsFound_KK6(string view) => Assert.NotEmpty(SortControl().Matches(view));

    [Theory]
    [InlineData("<a href=\"?page=2\">2</a>")]
    [InlineData("<input name=\"search\" />")]
    public void ThePageAndTheSearchParameters_AreNotASortControl_KK6(string view) => Assert.Empty(SortControl().Matches(view));

    // ---- KK9: the panel table has no selection column and no bulk action (04 2.13.6; K-761) ----

    [Fact]
    public void ThePanelTable_HasNoFormNoInputNoButtonAndNoCheckbox_KK9() => Assert.Empty(Control().Matches(View("_PanelTable")));

    [Theory]
    [InlineData("<td><input type=\"checkbox\" name=\"selected\" /></td>")]
    [InlineData("<th><input type=\"checkbox\" /></th>")]
    [InlineData("<form method=\"post\"><button>Toplu gönder</button></form>")]
    public void ASelectionCellOrABulkForm_IsFound_KK9(string view) => Assert.NotEmpty(Control().Matches(view));

    [Fact]
    public void ThePanelTable_IsNeverScrolledSidewaysAndNoCellIsHidden_KK9()
    {
        foreach (string file in new[] { "base.css", "components.css", "admin.css" })
        {
            string css = Css(file);
            Assert.Empty(SidewaysScroll().Matches(css).Select(m => m.Value));
            Assert.Empty(HiddenCellRules(css));
        }
    }

    private static IEnumerable<string> HiddenCellRules(string css) =>
        css.Split('}').Where(rule => rule.Contains("panel-table", StringComparison.Ordinal) && Regex.IsMatch(rule, @"display\s*:\s*none|visibility\s*:\s*hidden", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));

    [Theory]
    [InlineData(".panel-table { overflow-x: auto; }")]
    [InlineData(".panel-table__wrap { overflow: scroll; }")]
    public void ASidewaysScroll_IsFound_KK9(string css) => Assert.NotEmpty(SidewaysScroll().Matches(css));

    [Theory]
    [InlineData(".panel-table td.panel-table__cell--secondary { display: none; }")]
    [InlineData(".panel-table__cell { visibility: hidden; }")]
    public void AHiddenCell_IsFound_KK9(string css) => Assert.NotEmpty(HiddenCellRules(css + "}"));

    [Fact]
    public void ThePanelTableHead_IsKeptForReadersOnTheNarrowClass_NotRemoved_KK9() =>
        Assert.DoesNotContain(HiddenCellRules(Css("admin.css")), rule => rule.Contains("panel-table__head", StringComparison.Ordinal));

    // ---- KK5: two, three and four columns (04 2.11.5) ----

    private static Dictionary<int, int> GridColumns(string css) =>
        GridColumnRule().Matches(css).ToDictionary(m => m.Groups[1].Success ? int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : 0, m => int.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture));

    [Fact]
    public void TheCardGrid_IsTwoColumnsNarrow_ThreeMedium_FourWide_KK5() =>
        Assert.Equal(new Dictionary<int, int> { [0] = 2, [768] = 3, [1024] = 4 }, GridColumns(Css("components.css")));

    [Theory]
    [InlineData(".card-grid { grid-template-columns: repeat(2, 1fr); } @media (min-width: 768px) { .card-grid { grid-template-columns: repeat(3, 1fr); } }")]
    [InlineData(".card-grid { grid-template-columns: repeat(1, 1fr); } @media (min-width: 768px) { .card-grid { grid-template-columns: repeat(3, 1fr); } } @media (min-width: 1024px) { .card-grid { grid-template-columns: repeat(4, 1fr); } }")]
    [InlineData(".card-grid { grid-template-columns: repeat(2, 1fr); } @media (min-width: 768px) { .card-grid { grid-template-columns: repeat(3, 1fr); } } @media (min-width: 1024px) { .card-grid { grid-template-columns: repeat(5, 1fr); } }")]
    public void AGridThatIsNotTwoThreeFour_IsFound_KK5(string css) =>
        Assert.NotEqual(new Dictionary<int, int> { [0] = 2, [768] = 3, [1024] = 4 }, GridColumns(css));

    [Fact]
    public void TheRelatedProductsAndTheShowcase_UseTheGridAndTheCardOfTheCategoryPage_KK5()
    {
        // One grid and one card: the grid view draws the card view and nothing else draws product markup.
        Assert.Contains("<partial name=\"_ProductCard\"", View("_ProductGrid"), StringComparison.Ordinal);
        Assert.Contains("<partial name=\"_ContentCard\"", View("_ContentGrid"), StringComparison.Ordinal);
        Assert.DoesNotContain("product-card", string.Concat(ListViews.Where(v => v != "_ProductCard").Select(View)), StringComparison.Ordinal);
    }

    // ---- KK1: the one cut text of the card is its name, at two lines (04 8.3.2.2; K-741) ----

    private static List<string> LineClampViolations(string css) =>
        [.. css.Split('}').Where(rule => LineClamp().IsMatch(rule) && !rule.Contains(".product-card__name", StringComparison.Ordinal)).Select(rule => rule.Trim())];

    [Fact]
    public void TheLineClamp_IsUsedByTheProductCardNameOnly_AtTwoLines_KK1()
    {
        foreach (string file in new[] { "base.css", "components.css", "storefront.css", "admin.css" })
        {
            Assert.Empty(LineClampViolations(Css(file)));
        }

        string name = Css("components.css").Split('}').Single(rule => rule.Contains(".product-card__name", StringComparison.Ordinal) && LineClamp().IsMatch(rule));
        Assert.Matches(@"-webkit-line-clamp:\s*2;", name);
        Assert.Matches(@"(?<!-)line-clamp:\s*2;", name);
    }

    [Theory]
    [InlineData(".content-card__title { -webkit-line-clamp: 2; }")]
    [InlineData(".product-card__description { line-clamp: 3; }")]
    public void ALineClampOnAnotherText_IsFound_KK1(string css) => Assert.NotEmpty(LineClampViolations(css + "}"));

    // ---- KK8: no endless scroll (04 2.13.3; K-766) ----

    [Fact]
    public void NothingLoadsAListByScrolling_KK8()
    {
        Assert.Empty(ScrollLoading().Matches(ReadWeb("wwwroot/js/common.js")).Select(m => m.Value));
        foreach (string name in ListViews)
        {
            Assert.Empty(ScrollLoading().Matches(View(name)).Select(m => m.Value));
        }
    }

    [Theory]
    [InlineData("new IntersectionObserver(load);")]
    [InlineData("window.addEventListener('scroll', loadMore);")]
    [InlineData("<div data-js=\"load-more\"></div>")]
    public void AScrollLoader_IsFound_KK8(string text) => Assert.NotEmpty(ScrollLoading().Matches(text));

    // ---- KK8: the pager is numbered, previous and next (04 2.13.3) ----

    [Fact]
    public void ThePagerView_IsANavigationWithNumberedLinks_TheShownPageIsNotALink_KK8()
    {
        string pager = View("_Pager");

        Assert.Contains("<nav class=\"pager\"", pager, StringComparison.Ordinal);
        Assert.Contains("rel=\"prev\"", pager, StringComparison.Ordinal);
        Assert.Contains("rel=\"next\"", pager, StringComparison.Ordinal);
        Assert.Contains("aria-current=\"page\"", pager, StringComparison.Ordinal);
        Assert.Contains("@if (Model.IsShown)", pager, StringComparison.Ordinal);
    }
}
