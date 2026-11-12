using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The return address after a sign-in and the target of a sign-out (T207 criterion 6; KK6; 05 6.5.6; 07 2.3.2; 07 4.34.1; D-118): only a path of this site
/// passes, everything else is the area's home page. The table has the three forms of 05 6.5.6 and the control-character form a browser's address parser reads as
/// a network path.
/// </summary>
public sealed class ReturnPathTests
{
    [Theory]
    [InlineData("/hesap")]
    [InlineData("/panel/siparisler?page=2")]
    [InlineData("/")]
    [InlineData("/urun/ahsap-masa#yorumlar")]
    [InlineData("/a/b%2Fc")]
    public void IsLocalPath_OnASitePath_IsTrue(string value)
    {
        Assert.True(ReturnPath.IsLocalPath(value));
    }

    [Theory]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("https://evil.example")]
    [InlineData("http://evil.example/panel")]
    [InlineData("javascript:alert(1)")]
    [InlineData("evil")]
    [InlineData("evil.example/panel")]
    [InlineData("\\evil.example")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("/\t/evil.example")]
    [InlineData("/\n/evil.example")]
    [InlineData("/\r/evil.example")]
    [InlineData("/\u0001x")]
    [InlineData("/\u007Fx")]
    [InlineData("/ok\u0000")]
    [InlineData("\t//evil.example")]
    [InlineData(" /hesap")]
    public void IsLocalPath_OnAnythingElse_IsFalse(string? value)
    {
        Assert.False(ReturnPath.IsLocalPath(value));
    }

    [Theory]
    [InlineData("/hesap", "Storefront", "/hesap")]
    [InlineData("/panel/siparisler?page=2", "Panel", "/panel/siparisler?page=2")]
    [InlineData("//evil.example", "Storefront", "/")]
    [InlineData("//evil.example", "Panel", "/panel")]
    [InlineData("/\\evil.example", "Storefront", "/")]
    [InlineData("https://evil.example", "Panel", "/panel")]
    [InlineData("javascript:alert(1)", "Storefront", "/")]
    [InlineData("evil", "Panel", "/panel")]
    [InlineData("%2F%2Fevil.example", "Storefront", "/")]
    [InlineData("/\t/evil.example", "Storefront", "/")]
    [InlineData("/\u0001x", "Panel", "/panel")]
    [InlineData("", "Storefront", "/")]
    [InlineData(null, "Panel", "/panel")]
    public void Resolve_KeepsASitePath_AndAnswersTheAreasHomePageForAnythingElse(string? value, string area, string expected)
    {
        Assert.Equal(expected, ReturnPath.Resolve(value, Enum.Parse<ReturnArea>(area)));
    }

    [Fact]
    public void Resolve_OnAnUnknownArea_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ReturnPath.Resolve("evil", (ReturnArea)99));
    }

    [Theory]
    [InlineData("/urun/ahsap-masa", "/urun/ahsap-masa")]
    [InlineData("/siparis-takibi", "/siparis-takibi")]
    [InlineData("/siparis/erisim/abc_DEF-123", "/siparis/erisim/abc_DEF-123")]
    [InlineData("/hesap/silindi", "/hesap/silindi")]
    [InlineData("/hesap", "/")]
    [InlineData("/hesap/adresler", "/")]
    [InlineData("/hesap/adresler?returnUrl=%2F", "/")]
    [InlineData("/HESAP/adresler", "/")]
    [InlineData("/hesap/silindi/x", "/")]
    [InlineData("/panel", "/")]
    [InlineData("/panel/siparisler", "/")]
    [InlineData("/siparis/2026-7K4M9P", "/")]
    [InlineData("/siparis/2026-7K4M9P/iade", "/")]
    [InlineData("//evil.example", "/")]
    [InlineData("/\\evil.example", "/")]
    [InlineData("https://evil.example", "/")]
    [InlineData("/\t/evil.example", "/")]
    [InlineData("", "/")]
    [InlineData(null, "/")]
    [InlineData("/", "/")]
    public void AfterSignOut_KeepsAPageThatNeedsNoSession_AndAnswersTheHomePageForTheRest(string? value, string expected)
    {
        Assert.Equal(expected, ReturnPath.AfterSignOut(value));
    }

    [Theory]
    [InlineData("/urun/ahsap-masa", "/urun/ahsap-masa")]
    [InlineData("/sepet?x=1#a", "/sepet?x=1#a")]
    [InlineData("/giris", "/")]
    [InlineData("/giris?x", "/")]
    [InlineData("/giris/google", "/")]
    [InlineData("/Giris", "/")]
    [InlineData("/%67iris", "/")]
    [InlineData("/kayit", "/")]
    [InlineData("/sifre-sifirlama/abc", "/")]
    [InlineData("/dogrulama/abc", "/")]
    [InlineData("//evil.example", "/")]
    [InlineData("https://evil.example", "/")]
    [InlineData("", "/")]
    [InlineData(null, "/")]
    public void AfterSignIn_KeepsAPageOfTheSite_AndAnswersTheHomePageForAnEntryScreenOrForeignOrMissingValue(string? value, string expected)
    {
        Assert.Equal(expected, ReturnPath.AfterSignIn(value));
    }

    [Fact]
    public void TheHomePagesAndTheParameterName_AreTheDocumentedOnes()
    {
        Assert.Equal("/", ReturnPath.HomeOf(ReturnArea.Storefront));
        Assert.Equal("/panel", ReturnPath.HomeOf(ReturnArea.Panel));
        Assert.Equal("returnUrl", ReturnPath.ParameterName);
    }
}
