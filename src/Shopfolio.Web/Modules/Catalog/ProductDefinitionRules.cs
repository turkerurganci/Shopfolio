using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>What a product definition breaks (06 4.5 "Uygulamada"); the codes name the rule, the message is the guard's.</summary>
internal enum ProductDefinitionProblem
{
    ExemptionOutsideAPhysicalProduct,
    ServiceWithoutFulfilmentPeriodIsPublished,
}

/// <summary>
/// The type-dependent rules of the product definition as pure functions (02 3.2.2, 3.2.3, 7.3.4; 06 4.5; 11 T332 criteria 2, 9, 10). The type lives on the product and is a
/// closed three-valued field; the withdrawal exemption is the reason field being filled, only on a physical product, and it never enters the publication gate; a service
/// product carries a fulfilment period of at least <see cref="ParameterFences.FulfilmentPeriodLower"/> day and is not published without one (P-44). The rest of the gate
/// (name, main category, a published variant, the digital file) is <see cref="PublishGateRules"/>'s (T335), which asks this class for the fulfilment period - the rule stays here.
/// </summary>
internal static class ProductDefinitionRules
{
    /// <summary>The problems of a product as it would be saved; empty when it breaks none.</summary>
    public static IReadOnlyList<ProductDefinitionProblem> Check(
        ProductType type,
        PublishStatus status,
        WithdrawalExemptionReason? exemptionReason,
        int? fulfilmentPeriod)
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

        return problems;
    }

    /// <summary>Whether the fulfilment period is entered and within its lower fence: whole days, no default, never below the fence (P-44).</summary>
    public static bool IsFulfilmentPeriodEntered(int? fulfilmentPeriod) => fulfilmentPeriod >= ParameterFences.FulfilmentPeriodLower;
}
