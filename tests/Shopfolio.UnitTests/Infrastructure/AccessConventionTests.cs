using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The page convention of the closed doors (T207 criteria 2 and 3; KK2, KK3; 07 2.3.1; 09 15.1 item 2): a page of the panel folder has an address under
/// <c>/panel</c> and the other way round, the same for the account folder and <c>/hesap</c>; a page whose address is in the closed list of open pages gets the
/// allow-anonymous mark and no other page does. The convention is applied to a route model built by hand, which is what the framework hands it.
/// </summary>
public sealed class AccessConventionTests
{
    private static PageRouteModel Model(string viewEnginePath, params string[] templates)
    {
        PageRouteModel model = new("/Pages" + viewEnginePath + ".cshtml", viewEnginePath);
        foreach (string template in templates)
        {
            model.Selectors.Add(new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = template } });
        }

        return model;
    }

    private static bool IsOpen(string viewEnginePath, string template)
    {
        PageRouteModel model = Model(viewEnginePath, template);
        new AccessConvention().Apply(model);
        return model.Selectors.Single().EndpointMetadata.OfType<IAllowAnonymous>().Any();
    }

    [Theory]
    [InlineData("/Panel/SignIn", "panel/giris")]
    [InlineData("/Panel/Reset", "panel/sifre-sifirlama")]
    [InlineData("/Panel/Reset", "panel/sifre-sifirlama/{key}")]
    [InlineData("/Panel/Reset", "panel/sifre-sifirlama/{key?}")]
    [InlineData("/Panel/Invite", "panel/davet/{key}")]
    [InlineData("/Panel/Verify", "panel/dogrulama/{key}")]
    [InlineData("/Panel/Undo", "panel/geri-al/{key}")]
    [InlineData("/Account/Deleted", "hesap/silindi")]
    public void Apply_OnAnOpenPageOfTheClosedList_MarksItAllowAnonymous(string folderPath, string template)
    {
        Assert.True(IsOpen(folderPath, template));
    }

    [Theory]
    [InlineData("/Panel/Orders", "panel/siparisler")]
    [InlineData("/Panel/Orders", "panel")]
    [InlineData("/Panel/Reauth", "panel/yeniden-dogrulama")]
    [InlineData("/Panel/Invite", "panel/davet/{key?}")]
    [InlineData("/Panel/Invite", "panel/davet")]
    [InlineData("/Panel/Verify", "panel/dogrulama")]
    [InlineData("/Panel/SignIn", "panel/giris/{x}")]
    [InlineData("/Panel/Reset", "panel/sifre-sifirlama/{key}/{other}")]
    [InlineData("/Account/Home", "hesap")]
    [InlineData("/Account/Addresses", "hesap/adresler")]
    [InlineData("/Account/Deleted", "hesap/silindi/x")]
    [InlineData("/Account/Deleted", "hesap/silindi/{x}")]
    [InlineData("/Account/Other", "hesap/{x}")]
    public void Apply_OnAPageOutsideTheClosedList_LeavesItClosed(string folderPath, string template)
    {
        Assert.False(IsOpen(folderPath, template));
    }

    [Fact]
    public void Apply_OnAPageOutsideBothFolders_NeverMarksItAllowAnonymous()
    {
        Assert.False(IsOpen("/Storefront/Product", "urun/{slug}"));
        Assert.False(IsOpen("/Storefront/Home", string.Empty));
        Assert.False(IsOpen("/Storefront/SignIn", "giris"));
    }

    [Theory]
    [InlineData("/Storefront/Shop", "panel/siparisler")]
    [InlineData("/Account/Home", "panel/siparisler")]
    [InlineData("/Panel/Orders", "sepet")]
    [InlineData("/Panel/Orders", "hesap")]
    [InlineData("/Panel/Orders", "")]
    [InlineData("/Storefront/Cart", "hesap/adresler")]
    [InlineData("/Account/Home", "sepet")]
    [InlineData("/Account/Home", "")]
    [InlineData("/Panel/Home", "hesap")]
    public void Apply_OnAFolderAndAnAddressThatDoNotFit_ThrowsAndNamesThePageAndTheTemplate(string folderPath, string template)
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => new AccessConvention().Apply(Model(folderPath, template)));

        Assert.Contains(folderPath, error.Message, StringComparison.Ordinal);
        Assert.Contains($"'{template}'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_OnAPageWithoutATemplate_IsLeftAlone()
    {
        PageRouteModel model = new("/Pages/Panel/Orders.cshtml", "/Panel/Orders");
        model.Selectors.Add(new SelectorModel());

        Assert.Null(Record.Exception(() => new AccessConvention().Apply(model)));
    }

    [Fact]
    public void Apply_OnTwoSelectorsOfOnePage_MarksOnlyTheSelectorWhoseAddressIsOpen()
    {
        PageRouteModel model = Model("/Panel/Reset", "panel/sifre-sifirlama", "panel/sifre-sifirlama/{key}");

        new AccessConvention().Apply(model);

        Assert.All(model.Selectors, s => Assert.Single(s.EndpointMetadata.OfType<IAllowAnonymous>()));
    }

    [Theory]
    [InlineData("panel/giris", new[] { "/panel/giris" })]
    [InlineData("Panel/Giris", new[] { "/panel/giris" })]
    [InlineData("/panel/giris", new[] { "/panel/giris" })]
    [InlineData("panel/davet/{key}", new[] { "/panel/davet/{}" })]
    [InlineData("panel/davet/{key?}", new[] { "/panel/davet/{}", "/panel/davet" })]
    [InlineData("hesap/{x?}", new[] { "/hesap/{}", "/hesap" })]
    [InlineData("{x?}", new[] { "/{}", "/" })]
    [InlineData("urun/{slug:regex(^[a-z]+$)}", new[] { "/urun/{}" })]
    [InlineData("", new[] { "/" })]
    public void Variants_WritesTheAddressFormsTheWayTheOpenListDoes(string template, string[] expected)
    {
        Assert.Equal(expected, AccessConvention.Variants(template));
    }
}
