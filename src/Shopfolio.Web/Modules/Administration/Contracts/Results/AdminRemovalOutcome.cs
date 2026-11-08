namespace Shopfolio.Modules.Administration.Contracts.Results;

/// <summary>What became of the removal of an administrator (07 UC-177). Not stored.</summary>
public enum AdminRemovalOutcome
{
    Removed,

    /// <summary>The first send without the confirmation (200): nothing is written; the confirmation window says what the removal does.</summary>
    ConfirmationNeeded,

    /// <summary>The target is the acting administrator or the last one (422, <c>LastAdminRemoval</c>): the panel says why.</summary>
    Refused,

    /// <summary>The target is gone - removed meanwhile (404).</summary>
    NotFound,

    /// <summary>The acting administrator is no longer one: nothing is written and the request goes to the sign-in.</summary>
    ActorNotAdministrator,
}
