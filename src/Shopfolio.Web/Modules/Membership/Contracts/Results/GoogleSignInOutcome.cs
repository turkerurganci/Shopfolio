namespace Shopfolio.Modules.Membership.Contracts.Results;

/// <summary>How a Google sign-in ended (02 3.13.6, 3.13.7, 3.13.19). Not stored.</summary>
public enum GoogleSignInOutcome
{
    /// <summary>The installation has no Google application: the line is closed (02 3.13.6); nothing was read or written.</summary>
    Unavailable,

    /// <summary>The account may be given a session: it was already linked to Google, or Google is the authority of its address and the link was made now, or the account was just born.</summary>
    SignedIn,

    /// <summary>Google gave the e-mail as not verified: no link and no account; the screen sends the user to the registration with e-mail and password (02 3.13.7).</summary>
    EmailNotVerified,

    /// <summary>The matching account is not linked and Google is not the authority of the address: no link, no session; a confirmation link goes to the account's address (K-1281, K-1282).</summary>
    ConfirmationRequired,

    /// <summary>A new account would be opened and the data-collecting entries' gate is closed (02 3.13.19): nothing was written; a pending registration stays until its life ends.</summary>
    GateClosed,

    /// <summary>The identity is not usable (an empty or invalid e-mail).</summary>
    Refused,
}
