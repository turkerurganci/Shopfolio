using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The page title and description made from the record (T219 criterion 3, KK3; 05 3.7.2; 02 3.30.5): the title is the name and the brand name, the description is the
/// short field or the start of the body in plain form, cut at the ceiling without cutting a word, and the plain form of a formatted text has no tag, no script and
/// no entity.
/// </summary>
public sealed class SearchTextTests
{
    [Fact]
    public void Title_IsTheNameAnEnDashAndTheBrandName()
    {
        Assert.Equal("Ahşap Masa – Yılmaz Mobilya", SearchText.Title("Ahşap Masa", "Yılmaz Mobilya"));
        Assert.Equal("Ahşap Masa – Yılmaz Mobilya", SearchText.Title("  Ahşap Masa ", " Yılmaz Mobilya "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Title_ABrandNameNotFilledYet_LeavesTheNameAlone(string? brand)
    {
        Assert.Equal("Ahşap Masa", SearchText.Title("Ahşap Masa", brand));
    }

    [Fact]
    public void Title_ANameWithoutText_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => SearchText.Title(" ", "Marka"));
    }

    [Fact]
    public void Description_ThePlainShortField_IsTakenAsItIs()
    {
        Assert.Equal("Masif meşeden el yapımı masa.", SearchText.Description("Masif meşeden el yapımı masa.", "<p>Uzun gövde</p>"));
    }

    [Fact]
    public void Description_ABlankShortField_FallsBackToTheStartOfTheBodyInPlainForm()
    {
        Assert.Equal("Masif meşe & cila", SearchText.Description("  ", "<p>Masif <b>meşe</b> &amp; cila</p>"));
        Assert.Equal("Masif meşe", SearchText.Description(null, "<p>Masif</p><p>meşe</p>"));
    }

    [Fact]
    public void Description_AnEmptyRecord_IsNull()
    {
        Assert.Null(SearchText.Description(null, null));
        Assert.Null(SearchText.Description("", "<p> </p>"));
        Assert.Null(SearchText.Description(null, "<script>alert(1)</script>"));
    }

    [Fact]
    public void Description_ALongText_IsCutAtTheCeilingAtTheEndOfAWord()
    {
        string body = string.Join(' ', Enumerable.Repeat("sözcük", 60));

        string? description = SearchText.Description(null, $"<p>{body}</p>");

        Assert.NotNull(description);
        Assert.True(description.Length <= ProductConstants.ShortTextMaxLength);
        Assert.EndsWith("sözcük", description, StringComparison.Ordinal);
        Assert.True(description.Length > ProductConstants.ShortTextMaxLength - "sözcük".Length - 1, "The cut is at the last word that fits.");
        Assert.All(description.Split(' '), word => Assert.Equal("sözcük", word));
    }

    [Fact]
    public void Description_AShortFieldAtTheCeiling_IsNotCut()
    {
        string text = new('a', ProductConstants.ShortTextMaxLength);
        Assert.Equal(text, SearchText.Description(text, null));
    }

    [Fact]
    public void CutAtWord_AWordTheCeilingEndsExactlyAt_IsKeptWhole()
    {
        Assert.Equal("abc def", SearchText.CutAtWord("abc def ghi", 7));
        Assert.Equal("abc", SearchText.CutAtWord("abc def ghi", 6));
        Assert.Equal("abc def ghi", SearchText.CutAtWord("abc def ghi", 11));
    }

    [Fact]
    public void CutAtWord_AFirstWordLongerThanTheCeiling_IsCutAtTheCeiling_AndASurrogatePairIsNotSplit()
    {
        Assert.Equal("abcde", SearchText.CutAtWord("abcdefghij klm", 5));
        string emoji = "ab😀😀";
        string cut = SearchText.CutAtWord(emoji, 3);
        Assert.Equal("ab", cut);
    }

    [Fact]
    public void CategoryDescription_IsTheApplicationsOwnSentenceWithTheNameAndTheBrand()
    {
        Assert.Equal("Masalar kategorisindeki ürünleri Yılmaz Mobilya sitesinde inceleyin.", SearchText.CategoryDescription("Masalar", "Yılmaz Mobilya"));
        Assert.Equal("Masalar kategorisindeki ürünleri inceleyin.", SearchText.CategoryDescription("Masalar", null));
    }

    [Fact]
    public void CategoryDescription_ALongName_KeepsTheWholeWithinTheCeiling()
    {
        string description = SearchText.CategoryDescription(string.Join(' ', Enumerable.Repeat("uzunad", 60)), "Marka");
        Assert.True(description.Length <= ProductConstants.ShortTextMaxLength);
    }

    [Fact]
    public void ThePageDescriptionCeiling_IsTheShortFieldCeilingOfTheContentRecords()
    {
        Assert.Equal(ProductConstants.ShortTextMaxLength, Shopfolio.Modules.CorporateContent.ContentRules.ShortDescriptionMaxLength);
        Assert.Equal(200, ProductConstants.ShortTextMaxLength);
    }

    [Theory]
    [InlineData("<p>Bir</p><p>iki</p>", "Bir iki")]
    [InlineData("Satır<br>sonu<br/>iki", "Satır sonu iki")]
    [InlineData("<ul><li>a</li><li>b</li></ul>", "a b")]
    [InlineData("a<b>b</b>c", "abc")]
    [InlineData("<style>p{color:red}</style>Metin<!-- yorum -->", "Metin")]
    [InlineData("<script>alert('x')</script>Metin", "Metin")]
    [InlineData("<SCRIPT>alert('x')</SCRIPT >Metin", "Metin")]
    [InlineData("Fiyat &lt;5 &amp; &gt;2 &quot;iyi&quot;", "Fiyat <5 & >2 \"iyi\"")]
    [InlineData("  çok \n\t  boşluk  ", "çok boşluk")]
    [InlineData("5 < 6 ve 7 > 3", "5 < 6 ve 7 > 3")]
    public void PlainText_FromHtml_RemovesTagsScriptsAndEntities(string html, string expected)
    {
        Assert.Equal(expected, PlainText.FromHtml(html));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PlainText_FromHtml_AnEmptyValue_IsTheEmptyString(string? html)
    {
        Assert.Equal(string.Empty, PlainText.FromHtml(html));
    }
}
