using Microsoft.AspNetCore.Mvc;

using Shopfolio.Infrastructure.Forms;

namespace Shopfolio.Infrastructure.Routing;

/// <summary>
/// The registration of the address rules (07 2.1). The page convention is added to the Razor Pages options. The framework's route matching already
/// ignores the letter case of the literal segments and the trailing slash and does not redirect (07 2.1.7), and a parameter's value keeps its
/// letters: <c>RouteOptions.LowercaseUrls</c> stays off on purpose, because it would also lower the letters of a generated path's parameter values
/// - an access key or a link key is case sensitive. Addresses are lower case because the templates and the slugs are (07 2.1).
/// </summary>
internal static class AddressSetup
{
    public static IMvcBuilder AddAddressRules(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.Configure<MvcOptions>(options => options.Filters.Add(new HandlerRequiredFilter()));
        return builder.AddRazorPagesOptions(options => options.Conventions.Add(new AddressConvention()));
    }
}
