using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
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
/// (02 3.13.17). The reauthentication flow's return (T516) is <see cref="ReauthenticationReturnAsync"/>: it opens no session and ends at the account page or the re-authentication screen.
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
        if (flow is null)
        {
            return Back(NoticeCode.SocialLoginFailed);
        }

        // The kind of the flow is read from the protected state (07 UC-51): the re-authentication's return is another ending and reaches no sign-in.
        if (flow.State.Kind == GoogleFlowKind.Reauthentication)
        {
            return await ReauthenticationReturnAsync(flow, code, error, ct);
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

    /// <summary>
    /// The re-authentication flow's return (07 UC-51 "Yeniden doğrulama akışı"; 05 6.1.5; K-968). The signed-in customer's session is the one that asked, so no session is opened or changed: the answer
    /// is a 303 to the account page with the permission's single-use value when Google's fresh sign-in is after the start of the flow and its e-mail is the account's, else a 303 to the re-authentication
    /// screen with the operation and a code - <c>SocialReauthenticationUnavailable</c> when Google could not be reached, <c>SocialReauthenticationFailed</c> for every other ending (the user gave up, an error
    /// answer, a failed verification, a nonce that is not the flow's, a sign-in moment that is old or missing, another e-mail, an account not linked to Google). Nothing is written but the permission, and
    /// it does not count to L-1 (02 3.13.17). A state that names no operation of the closed list goes to the account page (07 6.2 ReauthenticationOperationInvalid).
    /// </summary>
    private async Task<IActionResult> ReauthenticationReturnAsync(GoogleFlow flow, string? code, string? error, CancellationToken ct)
    {
        if (!ReauthenticationAddresses.TryParse(flow.State.Operation, out ReauthenticationOperation operation))
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.ReauthenticationOperationInvalid);
            return FormResults.SeeOther(FramePaths.Account);
        }

        // The user gave up or Google answered with an error: no call to Google (D-1070; 08 2.3).
        if (!string.IsNullOrEmpty(error) || string.IsNullOrWhiteSpace(code))
        {
            return ReauthenticationBack(operation, NoticeCode.SocialReauthenticationFailed);
        }

        GoogleIdentityResult answer = await _flow.ExchangeAsync(flow.Values, code, ct);
        if (answer is not GoogleIdentityResult.Succeeded identity)
        {
            return ReauthenticationBack(operation, GoogleIdentityFailureRule.FailureOf(GoogleFlowKind.Reauthentication, answer) ?? NoticeCode.SocialReauthenticationFailed);
        }

        // No customer session: the screen is a page of the account folder, which sends the person to the sign-in (07 2.3.1).
        if (User.AccountId() is not Guid customerId || !Guid.TryParse(User.FindFirstValue(SessionSchemes.SessionIdClaim), out Guid sessionId))
        {
            return ReauthenticationBack(operation, NoticeCode.SocialReauthenticationFailed);
        }

        GoogleReauthenticationResult granted = await _transactions.RunAsync(
            async cancel => await _membership.GrantGoogleReauthenticationAsync(customerId, sessionId, operation, new GoogleReauthenticationIdentity(identity.Email, identity.EmailVerified, identity.AuthTime), flow.State.StartedAt, cancel),
            ct);
        return granted.Value is { } value
            ? FormResults.SeeOther(ReauthenticationAddresses.Granted(operation, value))
            : ReauthenticationBack(operation, NoticeCode.SocialReauthenticationFailed);
    }

    private static IActionResult ReauthenticationBack(ReauthenticationOperation operation, NoticeCode notice) => FormResults.SeeOther(ReauthenticationAddresses.Screen(operation), notice);

    private static IActionResult Back(NoticeCode notice) => FormResults.SeeOther(FramePaths.SignIn, notice);

    private sealed record GoogleOutcome(GoogleSignInResult Result, SessionOpening? Opening) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }
}
