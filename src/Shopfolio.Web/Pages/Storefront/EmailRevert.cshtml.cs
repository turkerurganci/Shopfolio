using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The landing screen of the "I did not make this change" link (T519; 07 4.28 UC-75, UC-76; 04 E-28): independent of a session. The GET only reads
/// (<see cref="IMembershipModule.ReadEmailRestoreLinkAsync"/>), so opening the link reverts nothing (K-904, K-839); the button's POST consumes the link with one conditional update, answers 303 to the
/// same address with the result code and ends the account's sessions - the browser's own cookie goes only when it is that account's. The earlier address having become another verified account's answers
/// 422 and the consumption rolls back (K-1210); a link spent in between answers 200 with the state it is in now. A key of another type, an administrator's link and an unknown key are the one invalid
/// state and nothing is consumed. The address holds a key, so no referrer is sent and the response is not stored (09 15.1 item 4).
/// </summary>
[KeyedLinkPage]
[NoStore]
internal sealed class EmailRevertModel : PageModel
{
    private readonly IMembershipModule _membership;
    private readonly ITransactionRunner _transactions;

    public EmailRevertModel(IMembershipModule membership, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(transactions);
        _membership = membership;
        _transactions = transactions;
    }

    /// <summary>The key of the link, as the address holds it: the button's form posts back to it.</summary>
    public string LinkKey { get; private set; } = string.Empty;

    /// <summary>What the page tells for the link's state now.</summary>
    public EmailRevertLanding Landing { get; private set; } = EmailRevertLandings.Of(VerificationLinkStatus.Missing, done: false, notApplied: false);

    public async Task<IActionResult> OnGetAsync(string key, CancellationToken ct)
    {
        EmailRevertLinkReading reading = await _membership.ReadEmailRestoreLinkAsync(key, ct);
        bool done = NoticeParameter.Read(Request, NoticeKind.Result) == NoticeCode.EmailChangeReverted;
        Show(key, reading.Status, done, notApplied: false);
        return Page();
    }

    public async Task<IActionResult> OnPostRevertPendingEmailChangeAsync(string key, CancellationToken ct)
    {
        // Read before the revert ends the sessions: whose session the browser holds, if any.
        AuthenticateResult session = await HttpContext.AuthenticateAsync(SessionSchemes.CustomerScheme);
        Guid? sessionAccount = session.Succeeded ? session.Principal?.AccountId() : null;
        RevertOutcome outcome = await _transactions.RunAsync(async cancel => new RevertOutcome(await _membership.RestorePreviousEmailAsync(key, cancel)), ct);

        if (outcome.Result.IsReverted)
        {
            // Only the reverted account's own session ends with the revert; the cookie of another account's session in this browser stays (04 5.28.8).
            if (sessionAccount is { } account && account == outcome.Result.CustomerId)
            {
                BrowserCookies.Delete(HttpContext, CookieNames.Session);
            }

            return FormResults.SeeOther(FramePaths.EmailRevert + "/" + Uri.EscapeDataString(key), NoticeCode.EmailChangeReverted);
        }

        if (outcome.Result.AddressTaken)
        {
            // The earlier address is another verified account's: nothing was written and the consumption is taken back (07 UC-76; K-1210).
            Show(key, VerificationLinkStatus.Valid, done: false, notApplied: true);
            return FormResults.Redisplay(this);
        }

        // The link is not (or no longer) valid: the page tells what it is now, status 200 (07 UC-76).
        EmailRevertLinkReading now = await _membership.ReadEmailRestoreLinkAsync(key, ct);
        Show(key, now.Status, done: false, notApplied: false);
        return Page();
    }

    private void Show(string key, VerificationLinkStatus status, bool done, bool notApplied)
    {
        LinkKey = key;
        Landing = EmailRevertLandings.Of(status, done, notApplied);
    }

    private sealed record RevertOutcome(EmailRevertResult Result) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }
}
