using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Lists;

/// <summary>
/// The way back from a record to its list (T210 criterion 9; KK9; 07 2.6.8; 05 6.5.6): the link carries the list's address in <c>returnUrl</c>, and the value is
/// read through the one local-path check - an address of another site, a network path, a backslash form, a control character, an empty or a missing value
/// returns to the list's root; a valid value gives back the page and the filter the user left.
/// </summary>
public sealed class ListReturnTests
{
    private const string Root = "/panel/siparisler";

    [Theory]
    [InlineData("/panel/siparisler?page=3")]
    [InlineData("/panel/siparisler?orderStatus=Paid&orderStatus=Shipped&page=2")]
    [InlineData("/panel/siparisler?search=a%26b&minPrice=1")]
    [InlineData("/panel/siparisler")]
    public void Target_OnTheListsOwnAddress_GivesItBack(string value)
    {
        Assert.Equal(value, ListReturn.Target(value, Root));
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("http://evil.example/panel/siparisler")]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("\\evil.example")]
    [InlineData("javascript:alert(1)")]
    [InlineData("evil")]
    [InlineData("/\t/evil.example")]
    [InlineData("/\n/evil.example")]
    [InlineData("/\u0001x")]
    [InlineData(" /panel/siparisler")]
    [InlineData("")]
    [InlineData(null)]
    public void Target_OnAnAddressThatIsNotThisSitesPath_OrOnNone_IsTheListRoot(string? value)
    {
        Assert.Equal(Root, ListReturn.Target(value, Root));
    }

    [Fact]
    public void Target_UsesTheOneLocalPathRule_OfTheSignInReturn()
    {
        // The same inputs give the same verdict as ReturnPath (T206, T207): a second rule would drift.
        string[] values = ["/ok", "//x", "/\\x", "/\t/x", "x", "", "/a?b=c#d"];
        foreach (string value in values)
        {
            Assert.Equal(ReturnPath.IsLocalPath(value), ListReturn.Target(value, Root) == value);
        }
    }

    [Fact]
    public void LinkTo_CarriesTheListAddress_EscapedAsOneQueryValue()
    {
        string link = ListReturn.LinkTo("/panel/siparis/0198", "/panel/siparisler?orderStatus=Paid&page=2");

        Assert.Equal("/panel/siparis/0198?returnUrl=%2Fpanel%2Fsiparisler%3ForderStatus%3DPaid%26page%3D2", link);
    }

    [Fact]
    public void LinkTo_ThenTarget_RoundTripsTheListAddress()
    {
        const string list = "/panel/siparisler?search=a%26b&orderStatus=Paid&orderStatus=Shipped&page=4";

        string link = ListReturn.LinkTo("/panel/siparis/0198", list);
        string carried = Uri.UnescapeDataString(link[(link.IndexOf("returnUrl=", StringComparison.Ordinal) + "returnUrl=".Length)..]);

        Assert.Equal(list, ListReturn.Target(carried, Root));
    }

    [Theory]
    [InlineData("https://evil.example", "/panel/siparisler")]
    [InlineData("/panel/siparis/1", "//evil.example")]
    [InlineData("panel/siparis/1", "/panel/siparisler")]
    public void LinkTo_OnAnAddressThatIsNotThisSitesPath_Throws(string record, string list)
    {
        Assert.Throws<ArgumentException>(() => ListReturn.LinkTo(record, list));
    }

    [Fact]
    public void ResolveOr_TakesAPathOfThisSiteOrTheFallback_AndTheFallbackMustBeOne()
    {
        Assert.Equal("/a", ReturnPath.ResolveOr("/a", "/b"));
        Assert.Equal("/b", ReturnPath.ResolveOr("//a", "/b"));
        Assert.Equal("/b", ReturnPath.ResolveOr(null, "/b"));
        Assert.Throws<ArgumentException>(() => ReturnPath.ResolveOr("/a", "https://evil.example"));
        Assert.Equal("/panel", ReturnPath.Resolve("//evil.example", ReturnArea.Panel));
        Assert.Equal("/", ReturnPath.Resolve(null, ReturnArea.Storefront));
    }
}
