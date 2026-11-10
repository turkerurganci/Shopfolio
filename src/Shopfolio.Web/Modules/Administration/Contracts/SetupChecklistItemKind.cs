namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>The five items of the setup checklist (02 10.8.2), in the order the sales gate lists its conditions.</summary>
public enum SetupChecklistItemKind
{
    /// <summary>The required identity fields of the chosen company type.</summary>
    CompanyIdentity,

    /// <summary>At least one open payment method (the IBAN while the transfer is on).</summary>
    PaymentMethod,

    /// <summary>The privacy notice, complete (the hosting location section included) and published.</summary>
    PrivacyNotice,

    /// <summary>The cookie policy, published.</summary>
    CookiePolicy,

    /// <summary>The return address.</summary>
    ReturnAddress,
}
