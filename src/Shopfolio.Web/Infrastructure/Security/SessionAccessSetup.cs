using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The registration of the two identity schemes and of the closed doors (09 15.1 item 2; 07 2.3.1; 05 6.2.2, 6.2.3): both schemes are
/// <see cref="SessionAuthenticationHandler"/>, the default scheme is the customer's, each policy carries its scheme and asks for an authenticated user, and the
/// page folders are closed by folder path, not by address (09 4.1 item 3): <c>AuthorizeFolder("/Panel", AdminPolicy)</c> and
/// <c>AuthorizeFolder("/Account", CustomerPolicy)</c>. The open pages are the closed list of 07 2.3.1 (<see cref="SessionSchemes.OpenPanelAddresses"/>,
/// <see cref="SessionSchemes.OpenAccountAddresses"/>), marked by <see cref="AccessConvention"/>.
/// </summary>
internal static class SessionAccessSetup
{
    public static IServiceCollection AddSessionAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddAuthentication(SessionSchemes.CustomerScheme)
            .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>(SessionSchemes.CustomerScheme, options =>
            {
                options.CookieName = CookieNames.Session;
                options.IsAdmin = false;
                options.SignInPath = SessionSchemes.CustomerSignInPath;
                options.Area = ReturnArea.Storefront;
            })
            .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>(SessionSchemes.AdminScheme, options =>
            {
                options.CookieName = CookieNames.AdminSession;
                options.IsAdmin = true;
                options.SignInPath = SessionSchemes.AdminSignInPath;
                options.Area = ReturnArea.Panel;
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(SessionSchemes.CustomerPolicy, policy => policy.AddAuthenticationSchemes(SessionSchemes.CustomerScheme).RequireAuthenticatedUser())
            .AddPolicy(SessionSchemes.AdminPolicy, policy => policy.AddAuthenticationSchemes(SessionSchemes.AdminScheme).RequireAuthenticatedUser());
        return services;
    }

    public static IMvcBuilder AddSessionAccess(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddRazorPagesOptions(options =>
        {
            options.Conventions.AuthorizeFolder(SessionSchemes.PanelFolder, SessionSchemes.AdminPolicy);
            options.Conventions.AuthorizeFolder(SessionSchemes.AccountFolder, SessionSchemes.CustomerPolicy);
            options.Conventions.Add(new AccessConvention());
        });
    }

    /// <summary>A machine endpoint under <c>/panel</c> (the chunked upload, 07 4.31.2) asks for the panel's session (07 2.3.1; K-947).</summary>
    public static TBuilder RequireAdminSession<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.RequireAuthorization(SessionSchemes.AdminPolicy);
    }

    /// <summary>
    /// Whether the request carries a valid administrator session (05 3.5.4): the storefront reads the panel's session for two jobs only, the admin strip and the
    /// preview of a draft record (07 2.2.6). It never makes the administrator a customer: <c>HttpContext.User</c> is the customer scheme's.
    /// </summary>
    public static async Task<bool> HasAdminSessionAsync(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AuthenticateResult result = await context.AuthenticateAsync(SessionSchemes.AdminScheme);
        return result.Succeeded;
    }
}

/// <summary>
/// The page convention of the closed doors (07 2.3.1; 09 15.1 item 2): a page of the panel folder has an address under <c>/panel</c> and the other way round, a
/// page of the account folder has an address under <c>/hesap</c> and the other way round - the folder closes the door, so an address in the wrong folder would be
/// open or closed by mistake and stops the routing instead. A page whose address is in the closed list of open pages gets the allow-anonymous mark; no other page
/// of the two folders does.
/// </summary>
internal sealed class AccessConvention : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        bool inPanel = IsUnder(model.ViewEnginePath, SessionSchemes.PanelFolder);
        bool inAccount = IsUnder(model.ViewEnginePath, SessionSchemes.AccountFolder);
        foreach (SelectorModel selector in model.Selectors)
        {
            string? template = selector.AttributeRouteModel?.Template;
            if (template is null)
            {
                continue;
            }

            IReadOnlyList<string> variants = Variants(template);
            string first = FirstSegment(variants[0]);
            if (first.Equals(SessionSchemes.PanelSegment, StringComparison.Ordinal) != inPanel)
            {
                throw new InvalidOperationException(
                    $"The page '{model.ViewEnginePath}' has the address template '{template}': the pages of the folder '{SessionSchemes.PanelFolder}' and only they have an address under '/{SessionSchemes.PanelSegment}' (07 2.3.1; 09 15.1 item 2).");
            }

            if (first.Equals(SessionSchemes.AccountSegment, StringComparison.Ordinal) != inAccount)
            {
                throw new InvalidOperationException(
                    $"The page '{model.ViewEnginePath}' has the address template '{template}': the pages of the folder '{SessionSchemes.AccountFolder}' and only they have an address under '/{SessionSchemes.AccountSegment}' (07 2.3.1; 09 15.1 item 2).");
            }

            IReadOnlyList<string> open = inPanel ? SessionSchemes.OpenPanelAddresses : inAccount ? SessionSchemes.OpenAccountAddresses : [];
            if (open.Count > 0 && variants.All(v => open.Contains(v, StringComparer.Ordinal)))
            {
                selector.EndpointMetadata.Add(new AllowAnonymousAttribute());
            }
        }
    }

    /// <summary>
    /// The address forms of a template as the open list writes them: lower case, with a leading slash, a route parameter as <c>{}</c>; a trailing optional
    /// parameter gives two forms, with and without the parameter.
    /// </summary>
    internal static IReadOnlyList<string> Variants(string template)
    {
        string trimmed = template.TrimStart('/');
        if (trimmed.Length == 0)
        {
            return ["/"];
        }

        RoutePattern pattern = RoutePatternFactory.Parse(trimmed);
        List<string> parts = [];
        bool lastOptional = false;
        foreach (RoutePatternPathSegment segment in pattern.PathSegments)
        {
            lastOptional = false;
            if (segment.Parts.Count == 1 && segment.Parts[0] is RoutePatternParameterPart parameter)
            {
                parts.Add("{}");
                lastOptional = parameter.IsOptional;
            }
            else
            {
                parts.Add(string.Concat(segment.Parts.Select(p => p is RoutePatternLiteralPart literal ? literal.Content.ToLowerInvariant() : "{}")));
            }
        }

        string full = "/" + string.Join('/', parts);
        return lastOptional ? [full, parts.Count == 1 ? "/" : "/" + string.Join('/', parts.Take(parts.Count - 1))] : [full];
    }

    private static string FirstSegment(string variant) => variant.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

    private static bool IsUnder(string viewEnginePath, string folder) =>
        viewEnginePath.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase);
}
