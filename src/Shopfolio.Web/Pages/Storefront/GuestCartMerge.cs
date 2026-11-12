using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The sign-in merge of the guest browser's cart into the member's (T515; 07 UC-55, UC-51; 02 3.16.3; D-558): one rule for every sign-in path, so the e-mail sign-in and the Google return call this and no
/// other. It runs in a transaction of its own after the session was opened and saved: the merge takes the guest cart's advisory lock and the account lock is a lock family of its own, so the two are never
/// held together (05 6.1.2, 7.3.5). The guest cart's cookie is deleted whether or not a cart matched - the browser is a member's now. A merge that adds what the member did not see marks the lines, and the
/// page the member lands on tells it once (07 4.34.3; K-1183).
/// </summary>
internal static class GuestCartMerge
{
    public static async Task RunAsync(HttpContext context, ITransactionRunner transactions, IOrderingModule ordering, Guid customerId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(ordering);
        string? guestKey = BrowserCookies.Read(context, CookieNames.Cart);
        if (string.IsNullOrWhiteSpace(guestKey))
        {
            return;
        }

        await transactions.RunAsync(async token => new MergeOutcome(await ordering.MergeGuestCartAsync(customerId, guestKey, token)), ct);
        BrowserCookies.Delete(context, CookieNames.Cart);
    }

    private sealed record MergeOutcome(CartMergeResult Result) : IOperationResult
    {
        public bool IsRejected => false;
    }
}
