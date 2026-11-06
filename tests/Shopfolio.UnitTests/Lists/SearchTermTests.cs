using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Lists;

/// <summary>
/// The search word as a <c>LIKE</c> pattern (T210 criterion 7; KK7; 07 2.6.3; 09 10.5 items 5, 6): blank is no word, the folded form, the cut at the
/// product-name limit before the escape, the escape of <c>\</c>, <c>%</c> and <c>_</c>.
/// </summary>
public sealed class SearchTermTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t  ")]
    [InlineData(" ")]
    public void ToLikePattern_OnABlankWord_IsNull_SoNoResultListIsMade(string? raw)
    {
        Assert.Null(SearchTerm.ToLikePattern(raw));
    }

    [Theory]
    [InlineData("masa", "%masa%")]
    [InlineData("  masa  ", "%masa%")]
    [InlineData("Isıtıcı", "%isitici%")]
    [InlineData("AMPUL", "%ampul%")]
    [InlineData("İŞÇİ ÇAĞRI", "%isci cagri%")]
    public void ToLikePattern_FoldsToTheSearchedForm(string raw, string expected)
    {
        Assert.Equal(expected, SearchTerm.ToLikePattern(raw));
    }

    [Theory]
    [InlineData("%", "%\\%%")]
    [InlineData("_", "%\\_%")]
    [InlineData("\\", "%\\\\%")]
    [InlineData("50%_off\\", "%50\\%\\_off\\\\%")]
    public void ToLikePattern_EscapesTheThreeWildcardCharacters(string raw, string expected)
    {
        Assert.Equal(expected, SearchTerm.ToLikePattern(raw));
    }

    [Fact]
    public void ToLikePattern_CutsALongWordToTheProductNameLimit_BeforeTheEscape()
    {
        int limit = ProductConstants.ProductNameMaxLength;

        string longWord = new('a', limit + 40);
        Assert.Equal("%" + new string('a', limit) + "%", SearchTerm.ToLikePattern(longWord));

        // A wildcard cut off is gone with its escape; one kept keeps the pair whole (a cut after the escape could split "\%").
        string wildcards = new string('a', limit - 1) + "%%%";
        Assert.Equal("%" + new string('a', limit - 1) + "\\%%", SearchTerm.ToLikePattern(wildcards));
    }

    [Fact]
    public void ToLikePattern_CutThroughASurrogatePair_DropsTheHalfPairToo()
    {
        int limit = ProductConstants.ProductNameMaxLength;
        string emoji = "\U0001F600";
        string word = new string('a', limit - 1) + emoji + "tail";

        string? pattern = SearchTerm.ToLikePattern(word);

        Assert.Equal("%" + new string('a', limit - 1) + "%", pattern);
    }

    [Fact]
    public void ToLikePattern_AWordOfTheLimitExactly_IsNotCut()
    {
        string word = new('b', ProductConstants.ProductNameMaxLength);
        Assert.Equal("%" + word + "%", SearchTerm.ToLikePattern(word));
    }
}
