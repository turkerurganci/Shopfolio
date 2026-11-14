using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Pages.Payment;

/// <summary>
/// UC-30 — the return from the payment provider (07 4.29.1; 08 2.1.4 item 3; T510): a one-sentence hand-over page without the site's header and footer that takes the top
/// window to the order's access-key page (<c>/siparis/erisim/{key}</c>, UC-34). The provider sends its frame here, so the page loads inside the site's own payment page and
/// only the site may frame it (<see cref="AllowFramingAttribute"/>: <c>frame-ancestors 'self'</c> on this response only); loaded in the top window it works the same way.
/// It writes nothing - the payment's result is the notification's and the last query's (05 4.5.3) -, reads no session cookie (the return is another site's request; K-906),
/// does not look the key up (an invalid or renewed key gets the order page's own 404) and reads none of the parameters the provider adds. The address's form is checked by
/// the route (<see cref="KeyPattern"/>, the access-key root's pattern): any other value is 404 and is never written into the page. The site's own script moves the top
/// window; without a script the <c>target="_top"</c> link stands (MK-05).
/// </summary>
[AllowFraming]
[KeyedLinkPage]
[NoStore]
public sealed class ReturnModel : PageModel
{
    /// <summary>The page's address template (07 UC-30): the return root of the link layer's return address and the access key in the access-key root's form.</summary>
    public const string Template = "/odeme/donus/{accessKey:regex(^[A-Za-z0-9_-]+\\z)}";

    /// <summary>The form of the key: the access-key root's (07 2.1.3; <see cref="OrderRoutes.AccessRoot"/>).</summary>
    public const string KeyPattern = "^[A-Za-z0-9_-]+\\z";

    /// <summary>04 3.5.8: the page's one sentence before its link.</summary>
    public const string SentenceText = "Siparişinize yönlendiriliyorsunuz; yönlendirilmezseniz";

    /// <summary>04 3.5.8: the link's text.</summary>
    public const string LinkText = "Siparişime git";

    /// <summary>The order's access-key page; the key went through the route's form check (no lookup here).</summary>
    public string OrderPath { get; private set; } = string.Empty;

    public void OnGet()
    {
        OrderPath = OrderRoutes.AccessPath(RouteData.Values["accessKey"] as string ?? string.Empty);
    }
}
