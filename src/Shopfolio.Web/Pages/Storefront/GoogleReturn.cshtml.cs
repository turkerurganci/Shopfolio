using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The return from Google (T515; 07 UC-51; 04 5.22.6, 5.22.10, 5.22.15; 05 6.1.4; 08 2.3). A GET that writes (07 UC-51): the flow cookie's one-time values bind the request, and a return that holds none - renewed,
/// foreign, with a <c>state</c> that does not hold - writes nothing and goes to the sign-in with <see cref="NoticeCode.SocialLoginFailed"/>. A <c>error</c> parameter or an empty <c>code</c> (the user gave up)
/// reaches no call to Google (D-1070). The identity is verified outside any transaction (the code exchange and the token check are the connector's, 09 11.3); the nonce is compared in constant time. The
/// account decision is the module's (<see cref="IMembershipModule.SignInWithGoogleAsync"/>); in the same transaction the guest orders of a newly born account are linked (D-577) and the session is opened after the
/// account is saved (D-556), the whole work run again when two first sign-ins of one address meet at the unique index. The cart merge is a transaction of its own (D-558). Google sign-in does not count to L-1
/// (02 3.13.17). The reauthentication flow is T516's: a return of that kind reaches no account here.
/// </summary>
[NoStore]
internal sealed class GoogleReturnModel : PageModel
{
    private readonly IMembershipModule _membership;
    private readonly ISessionService _sessions;
    private readonly IOrderingModule _ordering;
    private readonly ITransactionRunner _transactions;
    private readonly GoogleSignInFlow _flow;

    public GoogleReturnModel(
        IMembershipModule membership,
        ISessionService sessions,
        IOrderingModule ordering,
        ITransactionRunner transactions,
        GoogleSignInFlow flow)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(flow);
        _membership = membership;
        _sessions = sessions;
        _ordering = ordering;
        _transactions = transactions;
        _flow = flow;
    }

    public async Task<IActionResult> OnGetAsync(string? code, string? state, string? error, CancellationToken ct)
    {
        // The flow cookie is read and deleted first, whatever the return holds: its values are used once.
        GoogleFlow? flow = _flow.Consume(HttpContext, state);
        if (flow is null || flow.State.Kind != GoogleFlowKind.SignInOrRegistration)
        {
            return Back(NoticeCode.SocialLoginFailed);
        }

        // The user gave up or Google answered with an error: no call to Google (D-1070; 08 2.3 first row of the error table).
        if (!string.IsNullOrEmpty(error) || string.IsNullOrWhiteSpace(code))
        {
            return Back(NoticeCode.SocialLoginFailed);
        }

        GoogleIdentityResult.Succeeded? identity = await _flow.VerifyAsync(flow.Values, code, ct);
        if (identity is null)
        {
            // Unreachable, an error answer, a failed verification and a nonce that is not the flow's are all the same ending for the sign-in flow (08 2.3; 05 10.1.5).
            return Back(NoticeCode.SocialLoginFailed);
        }

        string? presentedMark = BrowserCookies.ReadKnownDevice(HttpContext);
        System.Net.IPAddress? address = ClientAddress.Of(HttpContext);
        GoogleOutcome outcome = await _transactions.RunAsync(
            async token =>
            {
                GoogleSignInResult result = await _membership.SignInWithGoogleAsync(new GoogleIdentity(identity.Email, identity.EmailVerified, identity.Name, identity.HostedDomain), address, token);
                if (result.CustomerId is not { } customerId || !result.IsSignedIn)
                {
                    return new GoogleOutcome(result, null);
                }

                // The account is saved before the session is written, under the account row's lock the module holds until the transaction ends (D-556). The session row takes the account's lock first,
                // so the guest orders - whose row locks follow the account's in the order of 05 7.3.5 - are linked after it, never before.
                SessionOpening opening = await _sessions.SignInAsync(customerId, false, presentedMark, token);
                if (result.AccountCreated)
                {
                    await _ordering.LinkGuestOrdersAsync(customerId, identity.Email, token);
                }

                return new GoogleOutcome(result, opening);
            },
            RegistrationIndexes.RetryOnUniqueIndexes,
            ct);

        GoogleSignInResult decided = outcome.Result;
        switch (decided.Outcome)
        {
            case GoogleSignInOutcome.SignedIn when outcome.Opening is { } opening && decided.CustomerId is { } customerId:
                BrowserCookies.Write(HttpContext, CookieNames.Session, opening.SessionKey, opening.CookieLifetime);
                BrowserCookies.WriteKnownDevice(HttpContext, opening.MarkValue, opening.MarkLifetime);
                await GuestCartMerge.MergeAsync(HttpContext, _transactions, _ordering, customerId, ct);
                return FormResults.SeeOther(ReturnPath.AfterSignIn(flow.State.ReturnUrl));
            case GoogleSignInOutcome.EmailNotVerified:
                return FormResults.SeeOther(FramePaths.Register, NoticeCode.SocialLoginEmailUnverified);
            case GoogleSignInOutcome.GateClosed:
                return Back(NoticeCode.SocialLoginDataCollectionClosed);
            case GoogleSignInOutcome.ConfirmationRequired:
                return Back(NoticeCode.SocialLoginLinkConfirmationSent);
            default:
                // The line is closed, or the identity is not usable: nothing was written (02 3.13.6).
                return Back(NoticeCode.SocialLoginFailed);
        }
    }

    private static IActionResult Back(NoticeCode notice) => FormResults.SeeOther(FramePaths.SignIn, notice);

    private sealed record GoogleOutcome(GoogleSignInResult Result, SessionOpening? Opening) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }
}
