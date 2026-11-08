using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>
/// What Google's verified identity hands the core (02 3.13.7; 08 2.3, 3.1 rule 4): the e-mail, whether Google gives it as verified, the name and the hosted domain
/// (<c>hd</c>). The subject identifier is not taken (K-1061). Checking the nonce and the sign-in moment is the connector's and the page's work (T409); the module decides what the
/// identity means for the accounts.
/// </summary>
public sealed record GoogleIdentity(string Email, bool EmailVerified, string? Name, string? HostedDomain);

/// <summary>
/// The outcome of a Google sign-in (02 3.13.6 to 3.13.8): the outcome, the account to give a session when there is one, whether the account was born now, whether the Google
/// link was made now, and - for a confirmation - whether a confirmation e-mail was queued (a valid link already waiting means none is made, K-1282). A sign-in with Google does not
/// count to the limit L-1 (02 3.13.17).
/// </summary>
public sealed class GoogleSignInResult : IOperationResult
{
    private GoogleSignInResult(GoogleSignInOutcome outcome, Guid? customerId, bool accountCreated, bool linkMade, bool pendingRegistrationReplaced, bool confirmationQueued, GateRefusal? gate, ErrorCode? code)
    {
        Outcome = outcome;
        CustomerId = customerId;
        AccountCreated = accountCreated;
        LinkMade = linkMade;
        PendingRegistrationReplaced = pendingRegistrationReplaced;
        ConfirmationQueued = confirmationQueued;
        Gate = gate;
        Code = code;
    }

    public GoogleSignInOutcome Outcome { get; }

    /// <summary>The account to open a session for; filled only for <see cref="GoogleSignInOutcome.SignedIn"/>, and the account of the confirmation for <see cref="GoogleSignInOutcome.ConfirmationRequired"/>.</summary>
    public Guid? CustomerId { get; }

    public bool AccountCreated { get; }

    /// <summary>The account was matched, was not linked and Google is the authority of its address: the link was made by this sign-in.</summary>
    public bool LinkMade { get; }

    /// <summary>The account is born from an unverified pending registration of the address that was deleted with its links (02 3.13.7; K-696).</summary>
    public bool PendingRegistrationReplaced { get; }

    /// <summary>True when the confirmation e-mail was queued; false when a valid confirmation link already waited and none was made.</summary>
    public bool ConfirmationQueued { get; }

    public GateRefusal? Gate { get; }

    public ErrorCode? Code { get; }

    public bool IsSignedIn => Outcome == GoogleSignInOutcome.SignedIn;

    /// <summary>
    /// True when nothing was written, so the caller's transaction is rolled back. A confirmation is not a refusal: it wrote the link and the e-mail's row, which must commit
    /// although no session is opened (K-1281).
    /// </summary>
    public bool IsRejected => Outcome is not (GoogleSignInOutcome.SignedIn or GoogleSignInOutcome.ConfirmationRequired);

    internal static GoogleSignInResult Unavailable() => new(GoogleSignInOutcome.Unavailable, null, false, false, false, false, null, null);

    internal static GoogleSignInResult NotVerified() => new(GoogleSignInOutcome.EmailNotVerified, null, false, false, false, false, null, null);

    internal static GoogleSignInResult Refused(ErrorCode code) => new(GoogleSignInOutcome.Refused, null, false, false, false, false, null, code);

    internal static GoogleSignInResult GateClosed(GateRefusal gate) => new(GoogleSignInOutcome.GateClosed, null, false, false, false, false, gate, gate.Code);

    internal static GoogleSignInResult Existing(Guid customerId, bool linkMade) => new(GoogleSignInOutcome.SignedIn, customerId, false, linkMade, false, false, null, null);

    internal static GoogleSignInResult Born(Guid customerId, bool pendingReplaced) => new(GoogleSignInOutcome.SignedIn, customerId, true, true, pendingReplaced, false, null, null);

    internal static GoogleSignInResult NeedsConfirmation(Guid customerId, bool queued) => new(GoogleSignInOutcome.ConfirmationRequired, customerId, false, false, false, queued, null, null);
}

/// <summary>The outcome of the confirmation button of the Google link (02 3.13.7; 07 UC-53): linked, or the link's state when it is no longer valid. A refusal writes nothing.</summary>
public sealed class GoogleLinkConfirmationResult : IOperationResult
{
    private GoogleLinkConfirmationResult(Guid? customerId, VerificationLinkStatus status)
    {
        CustomerId = customerId;
        Status = status;
    }

    public bool IsConfirmed => CustomerId is not null;

    /// <summary>The account that is now linked to Google; no session is opened (02 3.13.7).</summary>
    public Guid? CustomerId { get; }

    public VerificationLinkStatus Status { get; }

    public bool IsRejected => !IsConfirmed;

    internal static GoogleLinkConfirmationResult Confirmed(Guid customerId) => new(customerId, VerificationLinkStatus.Valid);

    internal static GoogleLinkConfirmationResult LinkInvalid(VerificationLinkStatus status) => new(null, status);
}
