using System.Globalization;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// Test-only endpoints in front of the application's pipeline (T224): they write cookies through the application's own writer, the way the later
/// tasks' pages will, and answer nothing else. They sit outside the cookie header filter, which is how a cookie written by any middleware reaches it.
/// </summary>
internal sealed class CookieProbeStartupFilter : IStartupFilter
{
    public const string AllPath = "/probe/all";
    public const string PanelPath = "/panel/probe";
    public const string CartPath = "/probe/cart";
    public const string CheckoutWritePath = "/probe/checkout-write";
    public const string CheckoutReadPath = "/probe/checkout-read";
    public const string CheckoutConfirmPath = "/probe/checkout-confirm";
    public const string FlowWritePath = "/probe/flow-write";
    public const string FlowConsumePath = "/probe/flow-consume";

    /// <summary>A checkout payload longer than one cookie holds, so the writer has to split it into chunks.</summary>
    public static readonly string LongPayload = string.Concat(Enumerable.Repeat("address-line;", 700));

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                BrowserCookies cookies = context.RequestServices.GetRequiredService<BrowserCookies>();
                string path = context.Request.Path.Value ?? string.Empty;
                switch (path)
                {
                    case AllPath:
                    case PanelPath:
                        WriteAll(context, cookies);
                        break;
                    case CartPath:
                        BrowserCookies.WriteGuestCart(context, "raw-cart-key");
                        break;
                    case CheckoutWritePath:
                        cookies.WriteCheckout(context, context.Request.Query["payload"].ToString());
                        break;
                    case CheckoutReadPath:
                        ProtectedValueResult read = cookies.ReadCheckout(context);
                        await context.Response.WriteAsync(read.IsValid ? "valid:" + read.Payload : "invalid", context.RequestAborted);
                        return;
                    case CheckoutConfirmPath:
                        BrowserCookies.DeleteCheckout(context);
                        break;
                    case FlowWritePath:
                        cookies.WriteExternalLoginFlow(context, "state=1;nonce=2");
                        break;
                    case FlowConsumePath:
                        ProtectedValueResult flow = cookies.ConsumeExternalLoginFlow(context);
                        await context.Response.WriteAsync(flow.IsValid ? "valid:" + flow.Payload : "invalid", context.RequestAborted);
                        return;
                    default:
                        break;
                }

                await nextMiddleware();
            });
            next(app);
        };
    }

    private static void WriteAll(HttpContext context, BrowserCookies cookies)
    {
        BrowserCookies.Write(context, CookieNames.Session, "session-key", TimeSpan.FromHours(2));
        BrowserCookies.Write(context, CookieNames.AdminSession, "admin-session-key");
        BrowserCookies.Write(context, CookieNames.KnownDevice, "mark", TimeSpan.FromDays(60));
        BrowserCookies.WriteGuestCart(context, "raw-cart-key");
        cookies.WriteCheckout(context, LongPayload);
        cookies.WriteExternalLoginFlow(context, "state=1;nonce=2");
        context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
    }
}

/// <summary>Reads the cookie headers of a response and checks them against the inventory (T224 KK1, KK2, KK3).</summary>
internal static class CookieHeaders
{
    public static List<SetCookieHeaderValue> Read(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (!response.Headers.TryGetValues(HeaderNames.SetCookie, out IEnumerable<string>? values))
        {
            return [];
        }

        return [.. values.Select(v => SetCookieHeaderValue.Parse(v))];
    }

    /// <summary>The name of the inventory a header belongs to: the framework's chunk cookies are the name followed by <c>C</c> and a number.</summary>
    public static string InventoryName(string headerName)
    {
        ArgumentNullException.ThrowIfNull(headerName);
        int end = headerName.Length;
        int digits = 0;
        while (end > 0 && char.IsAsciiDigit(headerName[end - 1]))
        {
            end--;
            digits++;
        }

        bool chunk = digits > 0 && end > 0 && headerName[end - 1] == 'C' && CookieNames.All.Contains(headerName[..(end - 1)], StringComparer.Ordinal);
        return chunk ? headerName[..(end - 1)] : headerName;
    }

    /// <summary>Returns what is wrong with the headers: a name outside the inventory, a missing prefix condition or a missing attribute; empty when all hold.</summary>
    public static List<string> Violations(IEnumerable<SetCookieHeaderValue> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        List<string> problems = [];
        foreach (SetCookieHeaderValue header in headers)
        {
            string name = header.Name.Value ?? string.Empty;
            if (!CookieNames.All.Contains(InventoryName(name), StringComparer.Ordinal))
            {
                problems.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: not in the cookie inventory"));
            }

            if (!name.StartsWith(CookieNames.Prefix, StringComparison.Ordinal))
            {
                problems.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: no __Host- prefix"));
            }

            if (!header.Secure)
            {
                problems.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: not Secure"));
            }

            if (!header.HttpOnly)
            {
                problems.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: not HttpOnly"));
            }

            if (header.SameSite != Microsoft.Net.Http.Headers.SameSiteMode.Lax)
            {
                problems.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: SameSite is {header.SameSite}, not Lax"));
            }

            if (!string.Equals(header.Path.Value, "/", StringComparison.Ordinal))
            {
                problems.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: Path is '{header.Path}', not '/'"));
            }

            if (header.Domain.HasValue)
            {
                problems.Add(string.Create(CultureInfo.InvariantCulture, $"{name}: has a Domain"));
            }
        }

        return problems;
    }
}
