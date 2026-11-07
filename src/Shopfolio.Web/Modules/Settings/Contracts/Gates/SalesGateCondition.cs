namespace Shopfolio.Modules.Settings.Contracts.Gates;

/// <summary>
/// The conditions of the sales gate, each by its name (02 3.1.5, 3.33.9, 6.9.10; 10 4.1 ÖK-8…ÖK-11): the panel and the refusals say which one is not met. The four
/// conditions of 02 3.1.5 are the identity, the payment method, the temporary closure and the four legal texts; the fourth is named by the part that is missing
/// (the privacy notice, the cookie policy, the return address that feeds the two generated texts), so the panel says which text waits.
/// </summary>
public enum SalesGateCondition
{
    /// <summary>The chosen company type's required identity fields are filled (condition 1).</summary>
    CompanyIdentity,

    /// <summary>At least one payment method is open (condition 2).</summary>
    PaymentMethod,

    /// <summary>The "close sales temporarily" switch is on (condition 3 is the switch being off).</summary>
    TemporaryClosure,

    /// <summary>The privacy notice is complete and published: the identity fields and the hosting location are filled (condition 4; the same definition as the data-collecting entries').</summary>
    PrivacyNotice,

    /// <summary>The cookie policy is published (condition 4).</summary>
    CookiePolicy,

    /// <summary>The return address that the two generated texts carry is entered (condition 4).</summary>
    ReturnAddress,
}
