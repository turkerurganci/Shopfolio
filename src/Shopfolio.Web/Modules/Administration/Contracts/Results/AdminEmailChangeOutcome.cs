namespace Shopfolio.Modules.Administration.Contracts.Results;

/// <summary>What became of the administrator's request to change the e-mail address (07 UC-179). Not stored.</summary>
public enum AdminEmailChangeOutcome
{
    Requested,

    /// <summary>The address is empty, malformed or another administrator account's (422, on <c>NewEmail</c>).</summary>
    FieldRefused,

    /// <summary>An open revert link closes the change (422, <c>EmailChangeRevertPending</c>); <see cref="AdminEmailChangeRequestResult.RevertEndsAt"/> says until when.</summary>
    RevertPending,

    /// <summary>L-9 over its limit on a request that replaces a pending one (429).</summary>
    Limited,

    /// <summary>The reauthentication value is missing, wrong or dropped (303 to the reauthentication page); nothing is written.</summary>
    ReauthenticationRequired,

    /// <summary>No such administrator.</summary>
    NotFound,
}
