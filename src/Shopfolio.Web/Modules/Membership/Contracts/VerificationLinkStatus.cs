namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>What the verification link shows when it is read (05 6.4.6): no usable record, or the record's state.</summary>
public enum VerificationLinkStatus
{
    /// <summary>No verification link carries the key: unknown, empty, or the key of another link type.</summary>
    Missing,

    Valid,

    /// <summary>The link did its work.</summary>
    Used,

    /// <summary>A newer link was asked for.</summary>
    Superseded,

    /// <summary>The link's lifetime ended: the page says the link is invalid and the visitor registers again (02 3.13.5).</summary>
    Expired,
}
