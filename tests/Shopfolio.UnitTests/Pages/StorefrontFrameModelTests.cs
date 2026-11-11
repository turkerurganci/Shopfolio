using Microsoft.AspNetCore.Http;

using Shopfolio.Pages.Frame;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The small rules of the storefront frame's model (T537; 07 5.5.1; 04 2.2.3, 2.2.7): the payment step's own path, the logo-or-name header and the member flag.</summary>
public sealed class StorefrontFrameModelTests
{
    [Theory]
    [InlineData("/odeme", true)]
    [InlineData("/odeme/", true)]
    [InlineData("/ODEME", true)]
    [InlineData("/odeme/donus", false)]
    [InlineData("/odemeler", false)]
    [InlineData("/sepet", false)]
    [InlineData("/", false)]
    public void IsCheckoutStep_IsTheExactPaymentPath_ButNotItsSubPaths(string path, bool expected)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;

        Assert.Equal(expected, StorefrontFrameBuilder.IsCheckoutStep(context.Request));
    }

    [Fact]
    public void FrameBrand_ShowsTheLogoOnlyWhenOneIsSet()
    {
        Assert.True(new FrameBrand("Marka", Guid.NewGuid(), null, "M", "#112233", "#FFFFFF").ShowsLogo);
        Assert.False(new FrameBrand("Marka", null, null, "M", "#112233", "#FFFFFF").ShowsLogo);
    }

    [Fact]
    public void FrameAccount_IsMemberOnlyWithAName()
    {
        Assert.True(new FrameAccount("Ayse", "/giris", "/siparis-takibi", "/hesabim", "/hesabim/siparisler", null).IsMember);
        Assert.False(new FrameAccount(null, "/giris", "/siparis-takibi", "/hesabim", "/hesabim/siparisler", null).IsMember);
    }
}
