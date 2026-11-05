using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The page convention of the address rules (T206 criterion 2; KK2; 07 2.1.2; 09 5.3 item 3): a page whose <c>@page</c> template is not in the closed
/// list of first segments stops the routing with the page named. The convention is applied to a route model built by hand, which is what the framework
/// hands it.
/// </summary>
public sealed class AddressConventionTests
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

    [Fact]
    public void Apply_OnAPageWithAClosedListTemplate_Passes()
    {
        new AddressConvention().Apply(Model("/Storefront/Cart", "sepet"));
        new AddressConvention().Apply(Model("/Storefront/Product", "urun/{slug}"));
        new AddressConvention().Apply(Model("/Storefront/Home", string.Empty));
        new AddressConvention().Apply(Model("/Panel/Orders/Index", "panel/siparisler"));
    }

    [Fact]
    public void Apply_OnAPageWithATemplateOutsideTheList_ThrowsAndNamesThePageAndTheRule()
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => new AddressConvention().Apply(Model("/Storefront/Shop", "magaza")));

        Assert.Contains("/Storefront/Shop", error.Message, StringComparison.Ordinal);
        Assert.Contains("magaza", error.Message, StringComparison.Ordinal);
        Assert.Contains("closed list", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_OnAPageWhoseTemplateIsItsPath_ThrowsBecauseTheAddressIsWrittenInThePageDirective()
    {
        // A page with a bare @page directive gets its path as the template ("Storefront/Cart"): upper case letters are not an address (09 5.3 item 3).
        Assert.Throws<InvalidOperationException>(() => new AddressConvention().Apply(Model("/Storefront/Cart", "Storefront/Cart")));
    }

    [Fact]
    public void Apply_ChecksEverySelectorOfThePage()
    {
        Assert.Throws<InvalidOperationException>(() => new AddressConvention().Apply(Model("/Storefront/Cart", "sepet", "alisveris-sepeti")));
    }

    [Fact]
    public void AddAddressRules_PutsTheConventionIntoTheRazorPagesOptions()
    {
        ServiceCollection services = new();
        services.AddRazorPages().AddAddressRules();
        using ServiceProvider provider = services.BuildServiceProvider();

        RazorPagesOptions options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RazorPagesOptions>>().Value;

        Assert.Contains(options.Conventions, c => c is AddressConvention);
    }
}
