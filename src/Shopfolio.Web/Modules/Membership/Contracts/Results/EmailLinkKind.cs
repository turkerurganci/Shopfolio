namespace Shopfolio.Modules.Membership.Contracts.Results;

/// <summary>Which of the account links the shared landing page (07 UC-52, E-21) opened: the registration's, the new address's or the Google confirmation's. Not stored.</summary>
public enum EmailLinkKind
{
    Registration,

    NewAddress,

    GoogleConfirmation,
}
