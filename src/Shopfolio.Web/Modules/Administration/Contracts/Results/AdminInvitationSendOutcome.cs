namespace Shopfolio.Modules.Administration.Contracts.Results;

/// <summary>What became of an invitation sent (07 UC-175; 02 10.2.1, 10.2.6). Not stored.</summary>
public enum AdminInvitationSendOutcome
{
    /// <summary>The invitation, its link, its e-mail, the trail row and the F-6 notice are written.</summary>
    Sent,

    /// <summary>The address is empty, malformed or an administrator account's (422, on the <c>Email</c> field); nothing is written.</summary>
    Refused,

    /// <summary>The acting administrator is no longer an administrator (removed meanwhile): nothing is written and the request goes to the sign-in (07 UC-177).</summary>
    ActorNotAdministrator,
}
