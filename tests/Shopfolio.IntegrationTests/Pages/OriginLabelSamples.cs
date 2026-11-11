using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>A test-only page that calls the one origin component the way the product page's price block does (T501 KK2, KK3, KK5): the product's type, the country's code and name come from the query.</summary>
public sealed class OriginLabelSampleModel : PageModel
{
    public OriginLabel? Label { get; private set; }

    public IActionResult OnGet(ProductType type, string? code, string? name)
    {
        Label = OriginLabel.Of(type, code, name);
        return Page();
    }
}
