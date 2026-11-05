using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Shopfolio.Infrastructure.Routing;

/// <summary>
/// The page convention of the address rules (07 2.1; 09 5.3 item 3: the address is always the <c>@page</c> template): every page route is checked
/// against <see cref="AddressTemplates"/> when the framework builds the page routes, and a page whose template breaks a rule stops the application's
/// routing with the page and the problems named, so a wrong address never answers a request.
/// </summary>
internal sealed class AddressConvention : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        foreach (SelectorModel selector in model.Selectors)
        {
            string? template = selector.AttributeRouteModel?.Template;
            if (template is null)
            {
                continue;
            }

            IReadOnlyList<string> problems = AddressTemplates.Check(template);
            if (problems.Count > 0)
            {
                throw new InvalidOperationException(
                    $"The page '{model.ViewEnginePath}' has the address template '{template}', which breaks the address rules (07 2.1): " + string.Join("; ", problems));
            }
        }
    }
}
