namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The registration of the cookie rules (09 15.1 item 6): the anti-forgery cookie takes its name and attributes from <see cref="CookieNames"/> and
/// <see cref="CookieDefaults"/>, the cookie writer and the policy gate are registered, and the filter that removes the storefront's cookie headers
/// until the cookie policy is published is put at the start of the pipeline.
/// </summary>
internal static class CookieSetup
{
    /// <summary>The header the anti-forgery token travels in when it is not in a form field (09 15.2 item 1).</summary>
    public const string AntiforgeryHeaderName = "RequestVerificationToken";

    public static IServiceCollection AddCookieRules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddAntiforgery(options =>
        {
            CookieDefaults.Apply(options.Cookie);
            options.Cookie.Name = CookieNames.Antiforgery;
            options.HeaderName = AntiforgeryHeaderName;
            // The framework would add X-Frame-Options to every response that carries a token; framing is the content security policy's alone (09 15.2 item 4).
            options.SuppressXFrameOptionsHeader = true;
        });
        services.AddSingleton<BrowserCookies>();
        services.AddSingleton<CookiePolicyGate>();
        return services;
    }

    public static IApplicationBuilder UseCookieHeaderFilter(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CookieHeaderFilterMiddleware>();
    }
}
