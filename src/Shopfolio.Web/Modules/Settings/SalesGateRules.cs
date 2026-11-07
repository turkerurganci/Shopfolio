using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// What the gate's decision is made from: plain values read from the database and from the conditions' sources, nothing else (05 4.1.5). The rule reads no clock, no database
/// and no request, so its boundary is tested with values.
/// </summary>
/// <param name="MissingIdentityFields">The identity fields that still wait (T330's completeness).</param>
/// <param name="PaymentMethodOpen">The card line or the bank transfer line is open.</param>
/// <param name="SalesTemporarilyClosed">The switch is on.</param>
/// <param name="PrivacyNoticePublishedWithHostingLocation">A privacy notice version is published with its hosting location filled.</param>
/// <param name="CookiePolicyPublished">A cookie policy version is published.</param>
/// <param name="ReturnAddressEntered">The four required parts of the return address are filled.</param>
internal readonly record struct SalesGateFacts(
    IReadOnlyList<string> MissingIdentityFields,
    bool PaymentMethodOpen,
    bool SalesTemporarilyClosed,
    bool PrivacyNoticePublishedWithHostingLocation,
    bool CookiePolicyPublished,
    bool ReturnAddressEntered);

/// <summary>
/// The one place the sales gate is decided (02 3.1.5, 3.1.6; 11 T331 criterion 8): from the facts it gives the unmet conditions by name. Every surface reads the
/// <see cref="SalesGate"/> this returns. The privacy notice is complete when the identity fields and the hosting location are filled and it is published - the
/// definition the data-collecting entries share with the fourth sales condition (02 3.1.5; 10 ÖK-10), so the two gates cannot differ.
/// </summary>
internal static class SalesGateRules
{
    public static SalesGate Evaluate(SalesGateFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts.MissingIdentityFields);
        bool identityComplete = facts.MissingIdentityFields.Count == 0;
        List<SalesGateCondition> unmet = [];
        if (!identityComplete)
        {
            unmet.Add(SalesGateCondition.CompanyIdentity);
        }

        if (!facts.PaymentMethodOpen)
        {
            unmet.Add(SalesGateCondition.PaymentMethod);
        }

        if (facts.SalesTemporarilyClosed)
        {
            unmet.Add(SalesGateCondition.TemporaryClosure);
        }

        if (!identityComplete || !facts.PrivacyNoticePublishedWithHostingLocation)
        {
            unmet.Add(SalesGateCondition.PrivacyNotice);
        }

        if (!facts.CookiePolicyPublished)
        {
            unmet.Add(SalesGateCondition.CookiePolicy);
        }

        if (!facts.ReturnAddressEntered)
        {
            unmet.Add(SalesGateCondition.ReturnAddress);
        }

        return new SalesGate(unmet, facts.MissingIdentityFields);
    }
}
