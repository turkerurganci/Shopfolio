namespace Shopfolio.Modules.Administration.Contracts.Results;

/// <summary>What became of the withdrawal of an invitation (07 UC-176). Not stored.</summary>
public enum AdminInvitationWithdrawalOutcome
{
    Withdrawn,

    /// <summary>The invitation was used, made invalid, withdrawn or ran out meanwhile (303, code <c>InvitationNoLongerActive</c>): nothing is written.</summary>
    NoLongerActive,

    /// <summary>The invitation's row is gone - the nightly deletion took it (404).</summary>
    NotFound,

    ActorNotAdministrator,
}
