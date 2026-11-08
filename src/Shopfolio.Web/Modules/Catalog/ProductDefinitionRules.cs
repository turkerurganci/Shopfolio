using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>What a product definition breaks (06 4.5 "Uygulamada"); the codes name the rule, the message is the guard's.</summary>
internal enum ProductDefinitionProblem
{
    ExemptionOutsideAPhysicalProduct,
    ServiceWithoutFulfilmentPeriodIsPublished,

    /// <summary>A physical product without its place of origin (02 3.8.5; 04 7.2.4.12; T336).</summary>
    PhysicalProductWithoutOrigin,

    /// <summary>A service product carrying a place of origin: a service has no such field (02 3.8.5; 04 9.5.22; T336).</summary>
    OriginOnAService,

    /// <summary>A shipping period above the upper fence of P-5 / P-6 (02 3.20.4; T350): a product's own period may not exceed the firm default's fence.</summary>
    ShippingPeriodOutsideTheFence,
}

/// <summary>
/// The type-dependent rules of the product definition as pure functions (02 3.2.2, 3.2.3, 7.3.4; 06 4.5; 11 T332 criteria 2, 9, 10). The type lives on the product and is a
/// closed three-valued field; the withdrawal exemption is the reason field being filled, only on a physical product, and it never enters the publication gate; a service
/// product carries a fulfilment period of at least <see cref="ParameterFences.FulfilmentPeriodLower"/> day and is not published without one (P-44). The rest of the gate
/// (name, main category, a published variant, the digital file) is <see cref="PublishGateRules"/>'s (T335), which asks this class for the fulfilment period - the rule stays here.
/// The place of origin is required on a physical product, optional on a digital one and absent on a service (02 3.8.5; T336 criterion 8) - in every status, as the form's
/// required field is.
/// </summary>
internal static class ProductDefinitionRules
{
    /// <summary>The problems of a product as it would be saved; empty when it breaks none.</summary>
    public static IReadOnlyList<ProductDefinitionProblem> Check(
        ProductType type,
        PublishStatus status,
        WithdrawalExemptionReason? exemptionReason,
        int? fulfilmentPeriod,
        Guid? originCountryId,
        int? shippingPeriod = null)
    {
        List<ProductDefinitionProblem> problems = [];
        if (exemptionReason is not null && type != ProductType.Physical)
        {
            problems.Add(ProductDefinitionProblem.ExemptionOutsideAPhysicalProduct);
        }

        if (type == ProductType.Service && status == PublishStatus.Published && !IsFulfilmentPeriodEntered(fulfilmentPeriod))
        {
            problems.Add(ProductDefinitionProblem.ServiceWithoutFulfilmentPeriodIsPublished);
        }

        if (originCountryId is null && PriceLabelRules.IsOriginRequired(type))
        {
            problems.Add(ProductDefinitionProblem.PhysicalProductWithoutOrigin);
        }

        if (originCountryId is not null && !PriceLabelRules.IsOriginAllowed(type))
        {
            problems.Add(ProductDefinitionProblem.OriginOnAService);
        }

        if (!CheckShippingPeriod(shippingPeriod))
        {
            problems.Add(ProductDefinitionProblem.ShippingPeriodOutsideTheFence);
        }

        return problems;
    }

    /// <summary>
    /// Whether a product's own shipping period is within the fence of P-6 (02 3.20.4, 3.20.5; 11 T350 criterion 5): no period at all (the product takes the firm default) is within; a number
    /// is checked against the fence the firm default has, 0..10 business days. The same question the panel's form asks (<see cref="FirmSettingFences"/>).
    /// </summary>
    public static bool CheckShippingPeriod(int? shippingPeriod)
    {
        return shippingPeriod is not { } days || FirmSettingFences.Check("P-6", days).Side == FenceSide.Within;
    }

    /// <summary>Whether the fulfilment period is entered and within its lower fence: whole days, no default, never below the fence (P-44).</summary>
    public static bool IsFulfilmentPeriodEntered(int? fulfilmentPeriod) => fulfilmentPeriod >= ParameterFences.FulfilmentPeriodLower;
}
