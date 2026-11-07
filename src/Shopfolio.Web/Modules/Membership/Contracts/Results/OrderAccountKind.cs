namespace Shopfolio.Modules.Membership.Contracts.Results;

/// <summary>Whose an order is (02 3.13.1, 3.13.3, 3.13.4).</summary>
public enum OrderAccountKind
{
    /// <summary>A guest order that belongs to no account; no verification is asked.</summary>
    Guest,

    /// <summary>The order of the signed-in, verified customer.</summary>
    SignedIn,

    /// <summary>A guest's order whose address is a verified account's: it is bound to that account at once, and it stays a guest order by how it was placed.</summary>
    GuestBoundToAccount,

    /// <summary>The signed-in customer's account is not verified: no order may be placed.</summary>
    RefusedUnverified,
}
