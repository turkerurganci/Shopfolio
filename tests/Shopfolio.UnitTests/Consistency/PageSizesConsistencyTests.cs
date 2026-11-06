using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the page sizes of <see cref="PageSizes"/> equal to the sentence of 04 2.13.3 (T210 criterion 2; 09 10.5 item 4; 09 21.4 item 1): the product lists of
/// the storefront, the content list page, the member's order history and the panel's lists. The expected sentence is built from the constants, so a size
/// changed in only one place goes red.
/// </summary>
[Trait("Consistency", "true")]
public sealed class PageSizesConsistencyTests
{
    [Fact]
    public void TheSentenceOf04_2_13_3_NamesTheSizesOfTheConstants()
    {
        string root = RepositoryRoot.Find();
        string document = File.ReadAllText(Path.Combine(root, "Docs", "04_UI_SPECS.md"));
        int start = document.IndexOf("**2.13.3 Uzun liste sayfalara bölünür**", StringComparison.Ordinal);
        Assert.True(start >= 0, "04 2.13.3 was not found.");
        int end = document.IndexOf('\n', start);
        string paragraph = document[start..end];

        Assert.Contains($"Vitrinin ürün listeleri sayfa başına {PageSizes.StorefrontProducts} ürün", paragraph, StringComparison.Ordinal);
        Assert.Contains($"içerik liste sayfası (E-07) {PageSizes.ContentList} kayıt", paragraph, StringComparison.Ordinal);
        Assert.Contains($"üyenin sipariş geçmişi (E-27) {PageSizes.OrderHistory} sipariş", paragraph, StringComparison.Ordinal);
        Assert.Contains($"panelin listeleri sayfa başına {PageSizes.Panel} kayıt", paragraph, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageSizesClass_HoldsExactlyTheFourSizes_SoANewSizeIsNotAddedWithoutTheTest()
    {
        string[] names = [.. typeof(PageSizes).GetFields().Select(f => f.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(["ContentList", "OrderHistory", "Panel", "StorefrontProducts"], names);
    }
}
