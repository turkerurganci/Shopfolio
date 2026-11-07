using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>What a stock or capacity value breaks for the product's type (02 3.6.2; 06 4.6 "Uygulamada").</summary>
internal enum StockProblem
{
    /// <summary>A physical product's variant has no stock number: the number is required.</summary>
    PhysicalWithoutStock,

    /// <summary>A digital product's variant carries a stock number: the field does not exist for it and it is sold without limit.</summary>
    DigitalWithStock,

    /// <summary>A number below zero: the stock is a whole number that never goes below zero (02 3.6.1).</summary>
    NegativeStock,
}

/// <summary>
/// The stock rules as pure functions (02 3.6.1-3.6.3; 11 T334 criteria 1-3): the stock is the variant's whole number; the type decides whether it is required (physical), absent
/// (digital) or optional with empty meaning unlimited (service); and a variant can be bought only while stock minus the reserved quantity is above zero. Nothing here reads the
/// database; the guard and the counters apply it.
/// </summary>
internal static class StockRules
{
    /// <summary>The problems of a stock value as it would be saved for a variant of the type; empty when it breaks none.</summary>
    public static IReadOnlyList<StockProblem> Check(ProductType type, int? stock)
    {
        List<StockProblem> problems = [];
        if (stock < 0)
        {
            problems.Add(StockProblem.NegativeStock);
        }

        if (type == ProductType.Physical && stock is null)
        {
            problems.Add(StockProblem.PhysicalWithoutStock);
        }

        if (type == ProductType.Digital && stock is not null)
        {
            problems.Add(StockProblem.DigitalWithStock);
        }

        return problems;
    }

    /// <summary>
    /// Whether the variant is shown as sold out (02 3.6.3): the count is tracked and the stock left after the reserved units is not above zero. An empty stock - a digital variant or
    /// a service without capacity - is never sold out. The reserved units count, so a last unit waiting for a payment is sold out now and returns if the payment does not come.
    /// </summary>
    public static bool IsSoldOut(int? stock, int reserved) => stock is not null && stock - reserved <= 0;

    /// <summary>
    /// Whether the product is shown as sold out (02 3.6.4): it has variants the visitor can see and every one of them is sold out. A product with no visible variant is not
    /// sold out; it is not for sale at all, which is the publication gate's matter.
    /// </summary>
    public static bool IsProductSoldOut(IReadOnlyCollection<bool> visibleVariantsSoldOut) => visibleVariantsSoldOut.Count > 0 && visibleVariantsSoldOut.All(s => s);
}
