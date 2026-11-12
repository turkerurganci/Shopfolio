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
/// The landing page of the e-mail links (T514; 07 4.9 UC-52, UC-53; 04 E-21): one address for the registration, new-address and Google confirmation links. The GET only reads
/// (<see cref="IMembershipModule.ReadEmailLinkAsync"/>), so opening a link verifies nothing (K-904, K-780); the button's POST consumes the link by its kind and answers with a 303 and a result code,
/// opens no session. A key of another type, an administrator's link and an unknown key are all the unrecognized state and nothing is consumed. The page does not depend on the session; only the
/// next step of the two new-address states does. The address holds a key, so no referrer is sent and the response is not stored (09 15.1 item 4).
/// </summary>
[KeyedLinkPage]
[NoStore]
internal sealed class VerificationModel : PageModel
{
    private readonly IMembershipModule _membership;
    private readonly ITransactionRunner _transactions;

    public VerificationModel(IMembershipModule membership, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(transactions);
        _membership = membership;
        _transactions = transactions;
    }

    /// <summary>The key of the link, as the address holds it: the page links back to it.</summary>
    public string LinkKey { get; private set; } = string.Empty;

    /// <summary>What the page tells for the link's state now.</summary>
    public VerificationLanding Landing { get; private set; } = VerificationLandings.Unrecognized;

    public async Task<IActionResult> OnGetAsync(string key, CancellationToken ct)
    {
        EmailLinkReading reading = await _membership.ReadEmailLinkAsync(key, ct);
        Show(key, reading, ResultCodeOf(reading), addressTaken: false);
        return Page();
    }

    public async Task<IActionResult> OnPostVerifyEmailAsync(string key, CancellationToken ct)
    {
        EmailLinkReading reading = await _membership.ReadEmailLinkAsync(key, ct);
        switch (reading.Kind)
        {
            case EmailLinkKind.Registration:
                EmailVerificationOutcome registration = await _transactions.RunAsync(async token => new EmailVerificationOutcome(await _membership.VerifyEmailAsync(key, token)), ct);
                if (registration.Result.IsVerified)
                {
                    return Done(key, NoticeCode.EmailVerified);
                }

                break;
            case EmailLinkKind.NewAddress:
                NewEmailOutcome newAddress = await _transactions.RunAsync(async token => new NewEmailOutcome(await _membership.VerifyNewEmailAsync(key, token)), ct);
                if (newAddress.Result.IsVerified)
                {
                    return Done(key, NoticeCode.EmailVerified);
                }

                if (newAddress.Result.AddressTaken)
                {
                    // The address became a verified account's meanwhile: the consumption is rolled back, nothing is written (07 UC-53; K-1080).
                    EmailLinkReading kept = await _membership.ReadEmailLinkAsync(key, ct);
                    Show(key, kept, null, addressTaken: true);
                    return FormResults.Redisplay(this);
                }

                break;
            case EmailLinkKind.GoogleConfirmation:
                GoogleConfirmationOutcome google = await _transactions.RunAsync(async token => new GoogleConfirmationOutcome(await _membership.ConfirmGoogleLinkAsync(key, token)), ct);
                if (google.Result.IsConfirmed)
                {
                    return Done(key, NoticeCode.GoogleLinkConfirmed);
                }

                break;
            default:
                break;
        }

        // The link is not (or no longer) valid: the page tells what it is now, status 200 (07 UC-53).
        EmailLinkReading now = await _membership.ReadEmailLinkAsync(key, ct);
        Show(key, now, null, addressTaken: false);
        return Page();
    }

    private static IActionResult Done(string key, NoticeCode notice) => FormResults.SeeOther(FramePaths.Verification + "/" + Uri.EscapeDataString(key), notice);

    // The result code counts only for the link kind it belongs to; Show then counts it only on a used link.
    private NoticeCode? ResultCodeOf(EmailLinkReading reading)
    {
        NoticeCode? code = NoticeParameter.Read(Request, NoticeKind.Result);
        return (reading.Kind, code) switch
        {
            (EmailLinkKind.Registration or EmailLinkKind.NewAddress, NoticeCode.EmailVerified) => code,
            (EmailLinkKind.GoogleConfirmation, NoticeCode.GoogleLinkConfirmed) => code,
            _ => null,
        };
    }

    private void Show(string key, EmailLinkReading reading, NoticeCode? resultCode, bool addressTaken)
    {
        LinkKey = key;
        bool isMember = HttpContext.GetStorefrontFrame()?.Account.IsMember ?? false;
        Landing = VerificationLandings.Of(reading, resultCode is not null, addressTaken, isMember);
    }

    private sealed record EmailVerificationOutcome(EmailVerificationResult Result) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }

    private sealed record NewEmailOutcome(NewEmailVerificationResult Result) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }

    private sealed record GoogleConfirmationOutcome(GoogleLinkConfirmationResult Result) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }
}
