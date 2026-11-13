using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Pages.Orders;

/// <summary>
/// The one lookup of the two order roots (07 2.1.3, 2.3.2-2.3.5; T511): the order page and the customer's operation screens under it (E-17, E-18, E-19) find the order the same way, so a
/// screen can never be open where the page is not. The member's root: no session at all is 302 to the sign-in with the return path (the customer scheme's challenge); an administrator's
/// session is not a customer (07 2.3.5) and is the one 404, like an order of another account, an order linked to no account or a number that does not exist. The access-key root: a key that
/// is not valid, a renewed key and the order whose personal data was disposed are the one 404 (04 5.16.20).
/// </summary>
internal static class OrderRoots
{
    /// <summary>The member's root: the order of the signed-in account, by its number.</summary>
    public static async Task<OrderRootResult> ResolveMemberAsync(PageModel page, IOrderingModule ordering, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(ordering);
        if (page.User.AccountId() is not Guid customerId)
        {
            return OrderRootResult.Denied(await page.HttpContext.HasAdminSessionAsync()
                ? AccessDenial.NotFound()
                : page.Challenge(SessionSchemes.CustomerScheme));
        }

        string number = page.RouteData.Values["orderNumber"] as string ?? string.Empty;
        Guid? orderId = await ordering.FindMemberOrderAsync(number, customerId, ct);
        return orderId is Guid id ? OrderRootResult.Found(id) : OrderRootResult.Denied(AccessDenial.NotFound());
    }

    /// <summary>The access-key root: the order of the key, open to the guest and to the member alike.</summary>
    public static async Task<OrderRootResult> ResolveAccessAsync(PageModel page, IOrderingModule ordering, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(ordering);
        string key = page.RouteData.Values["accessKey"] as string ?? string.Empty;
        Guid? orderId = await ordering.FindOrderByAccessKeyAsync(key, ct);
        return orderId is Guid id ? OrderRootResult.Found(id) : OrderRootResult.Denied(AccessDenial.NotFound());
    }

    /// <summary>
    /// The root a screen was reached from (07 2.1.3): the screen's address without its last segment, as written - the key is case sensitive (07 2.1.7). The answers of the screen go back
    /// to this root.
    /// </summary>
    public static string RootOf(string screenPath, string screen)
    {
        ArgumentNullException.ThrowIfNull(screenPath);
        ArgumentException.ThrowIfNullOrEmpty(screen);
        if (!OrderRoutes.Screens.Contains(screen, StringComparer.Ordinal))
        {
            throw new ArgumentException("The screen is not one of the order screens (07 2.1.3).", nameof(screen));
        }

        string suffix = "/" + screen;
        return screenPath.EndsWith(suffix, StringComparison.Ordinal) ? screenPath[..^suffix.Length] : screenPath;
    }
}
