namespace Shopfolio.Modules.Administration.Contracts.Results;

/// <summary>What became of the invitee's button (07 UC-113). Not stored.</summary>
public enum AdminInvitationAcceptanceOutcome
{
    /// <summary>The account is born, the link is used and the invitation accepted.</summary>
    Created,

    /// <summary>The name or the password is refused (422): the link is not consumed.</summary>
    FieldRefused,

    /// <summary>The link is not valid (200): used, withdrawn, made invalid or ran out - or the address became an administrator's meanwhile, when the invitation is made invalid (K-1085).</summary>
    LinkInvalid,
}
