using Microsoft.AspNetCore.Mvc;

namespace Shopfolio.Infrastructure.Lists;

/// <summary>The registration of the list pattern (07 2.6): the filter that marks a storefront list page closed to the search engines when it carries a filter (07 2.6.7).</summary>
internal static class ListSetup
{
    public static IMvcBuilder AddListPattern(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.Configure<MvcOptions>(options => options.Filters.Add<ListIndexingFilter>());
        return builder;
    }
}
