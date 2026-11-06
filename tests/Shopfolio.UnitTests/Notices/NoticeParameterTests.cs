using Microsoft.AspNetCore.Http;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;

namespace Shopfolio.UnitTests.Notices;

/// <summary>
/// The <c>notice</c> parameter (T214 criteria 1-3, 5; KK1, KK2; 07 2.2.3, 2.6.6, 6.3): the closed list reads only a member's exact name, a value outside the
/// list is ignored, a member of another list is not read in the place of the asked one, and the address carries the code and nothing else.
/// </summary>
public sealed class NoticeParameterTests
{
    private const string N = NoticeParameter.ParameterName;

    private static DefaultHttpContext Request(string query)
    {
        DefaultHttpContext context = new();
        context.Request.QueryString = new QueryString(query);
        return context;
    }

    [Fact]
    public void EveryMember_IsReadByItsExactName_AndInItsOwnListOnly_KK1()
    {
        foreach (NoticeCode code in Enum.GetValues<NoticeCode>())
        {
            HttpRequest request = Request($"?{NoticeParameter.ParameterName}={code}").Request;

            foreach (NoticeKind kind in Enum.GetValues<NoticeKind>())
            {
                Assert.Equal(NoticeKinds.Of(code) == kind ? code : null, NoticeParameter.Read(request, kind));
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("saved")]
    [InlineData("SAVED")]
    [InlineData(" Saved")]
    [InlineData("Saved ")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("-1")]
    [InlineData("999")]
    [InlineData("Saved,CartItemAdded")]
    [InlineData("Saved%2CCartItemAdded")]
    [InlineData("Saved|CartItemAdded")]
    [InlineData("Saved%00")]
    [InlineData("%3Cscript%3Ealert(1)%3C/script%3E")]
    [InlineData("Kayit%20basarili")]
    [InlineData("ErrorCode")]
    [InlineData("NotFound")]
    [InlineData("FieldRequired")]
    public void AValueOutsideTheClosedList_IsIgnored_ForEveryList_KK1_KK2(string value)
    {
        HttpRequest request = Request("?" + NoticeParameter.ParameterName + "=" + value).Request;

        Assert.All(Enum.GetValues<NoticeKind>(), kind => Assert.Null(NoticeParameter.Read(request, kind)));
    }

    [Fact]
    public void ARepeatedOrAbsentParameter_IsIgnored()
    {
        Assert.Null(NoticeParameter.Read(Request($"?{N}={NoticeCode.Saved}&{N}={NoticeCode.Saved}").Request, NoticeKind.ShortLived));
        Assert.Null(NoticeParameter.Read(Request($"?{N}={NoticeCode.Saved}&{N}={NoticeCode.CartItemAdded}").Request, NoticeKind.ShortLived));
        Assert.Null(NoticeParameter.Read(Request("?page=2").Request, NoticeKind.ShortLived));
        Assert.Null(NoticeParameter.Read(Request(string.Empty).Request, NoticeKind.ShortLived));
        // The framework reads a query key without regard to its letter case (07 2.6.1): the parameter name is read the way every other name is.
        Assert.Equal(NoticeCode.Saved, NoticeParameter.Read(Request($"?{char.ToUpperInvariant(N[0])}{N[1..]}={NoticeCode.Saved}").Request, NoticeKind.ShortLived));
    }

    [Fact]
    public void TheParameterName_IsNotice_AsThe07Says()
    {
        Assert.Equal("notice", NoticeParameter.ParameterName);
    }

    [Theory]
    [InlineData("/sepet", "/sepet?")]
    [InlineData("/sepet?page=2", "/sepet?page=2&")]
    [InlineData("/panel/siparisler?search=a%20b&page=2", "/panel/siparisler?search=a%20b&page=2&")]
    [InlineData("/sepet?", "/sepet?")]
    [InlineData("/sepet?x=1&", "/sepet?x=1&")]
    public void TheAddress_CarriesTheCodeAfterTheOwnQuery_AndNothingElse_KK2(string path, string expectedBase)
    {
        foreach (NoticeCode code in Enum.GetValues<NoticeCode>())
        {
            string address = NoticeParameter.Address(path, code);

            string expectedPrefix = expectedBase + N + "=";

            Assert.Equal(expectedPrefix + code, address);
            Assert.Matches("^[A-Za-z]+$", address[expectedPrefix.Length..]);
        }
    }

    [Fact]
    public void TheAddress_IsReadBackAsTheSameCode()
    {
        foreach (NoticeCode code in Enum.GetValues<NoticeCode>())
        {
            string address = NoticeParameter.Address("/sepet?page=2", code);
            DefaultHttpContext context = Request(address[address.IndexOf('?', StringComparison.Ordinal)..]);

            Assert.Equal(code, NoticeParameter.Read(context.Request, NoticeKinds.Of(code)));
        }
    }

    [Theory]
    [InlineData("sepet")]
    [InlineData("//dis.example/sepet")]
    [InlineData("/\\dis.example")]
    [InlineData("https://dis.example/")]
    [InlineData("")]
    [InlineData("/sepet\t")]
    [InlineData("/sepet#bolum")]
    [InlineData("/sepet?notice=Saved")]
    [InlineData("/sepet?page=2&Notice=Saved")]
    public void TheAddress_RefusesAPathThatIsNotTheSitesOwn_OrAlreadyCarriesANotice(string path)
    {
        Assert.Throws<ArgumentException>(() => NoticeParameter.Address(path, NoticeCode.Saved));
    }

    [Fact]
    public void TheAddress_RefusesAValueThatIsNotAMember()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NoticeParameter.Address("/sepet", (NoticeCode)999));
    }

    [Fact]
    public async Task TheSeeOtherResult_IsA303ToTheAddressWithTheCode_AndSetsNoCookie_KK3()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        await FormResults.SeeOther("/sepet?page=2", NoticeCode.CartItemAdded).ExecuteResultAsync(new Microsoft.AspNetCore.Mvc.ActionContext { HttpContext = context });

        Assert.Equal(StatusCodes.Status303SeeOther, context.Response.StatusCode);
        Assert.Equal($"/sepet?page=2&{N}={NoticeCode.CartItemAdded}", context.Response.Headers.Location.ToString());
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
        Assert.Equal(["Location"], context.Response.Headers.Keys);
    }

    [Fact]
    public void ARedirectWithANotice_StillRefusesANonLocalPath()
    {
        Assert.Throws<ArgumentException>(() => FormResults.SeeOther("//dis.example", NoticeCode.Saved));
    }
}
