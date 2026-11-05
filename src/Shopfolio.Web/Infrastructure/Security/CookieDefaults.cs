using Microsoft.AspNetCore.Http;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one place the attributes of a cookie are written (09 15.1 item 6; 05 6.7.1): Secure, HttpOnly, SameSite=Lax, the path <c>/</c> and no Domain
/// — the conditions of the <c>__Host-</c> prefix. Every cookie builder the application sets (the anti-forgery cookie, the session cookies, the flow
/// cookie) goes through <see cref="Apply"/>; the cookies written by hand take their <see cref="CookieOptions"/> from the builder's
/// <c>Build(HttpContext)</c>. SameSite is Lax and not Strict because a person who arrives from a link in an e-mail must still be seen as signed in
/// (05 6.7.11), and not the framework's external sign-in default (None) because the return from Google is a GET (05 6.7.1).
/// </summary>
internal static class CookieDefaults
{
    /// <summary>Sets the fixed attributes on a builder; the name and the life stay with the caller.</summary>
    public static CookieBuilder Apply(CookieBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.SecurePolicy = CookieSecurePolicy.Always;
        builder.HttpOnly = true;
        builder.SameSite = SameSiteMode.Lax;
        builder.Path = "/";
        builder.Domain = null;
        builder.IsEssential = true;
        return builder;
    }

    /// <summary>A builder for one cookie of the inventory; <paramref name="lifetime"/> empty means the cookie lasts as long as the browser session.</summary>
    public static CookieBuilder Create(string name, TimeSpan? lifetime = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!CookieNames.All.Contains(name, StringComparer.Ordinal))
        {
            throw new ArgumentException("The name is not in the closed list of CookieNames.", nameof(name));
        }

        return Apply(new CookieBuilder { Name = name, MaxAge = lifetime });
    }
}
