using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The registration of the form and site security (09 15.2): the page filter and the policy marker of the forgery protection, the <c>no-store</c> folder
/// conventions of the panel and the account, and the security headers middleware. Every page POST is checked without a mark on the page (05 6.5.5: a
/// forgotten page is protected, not open); the only way out is the endpoints of <see cref="AntiforgeryExemptions"/> (the framework's ignore attribute does not
/// switch the check off, and a source scan keeps it out of the application).
/// </summary>
internal static class ForgerySetup
{
    public static IMvcBuilder AddFormSecurity(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddScoped<ReauthenticationValues>();
        builder.Services.TryAddSingleton<RejectionLog>();
        builder.Services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(new ForgeryProtectionPolicy());
            // An explicit order: the type filter's own order is 0 whatever the class says, and the check must run before the field validation (also 0, registered earlier).
            options.Filters.Add<ForgeryProtectionFilter>(ForgeryProtectionFilter.FilterOrder);
        });

        // The personal pages: the whole panel and the whole account folder (09 15.2 item 5). The cart, the payment step and the keyed link pages carry the attribute themselves.
        return builder.AddRazorPagesOptions(options =>
        {
            options.Conventions.AddFolderApplicationModelConvention(SessionSchemes.PanelFolder, model => model.Filters.Add(new NoStoreAttribute()));
            options.Conventions.AddFolderApplicationModelConvention(SessionSchemes.AccountFolder, model => model.Filters.Add(new NoStoreAttribute()));
        });
    }

    /// <summary>The security headers of every response: the first middleware of the pipeline after the forwarded headers (09 15.2 item 3).</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
