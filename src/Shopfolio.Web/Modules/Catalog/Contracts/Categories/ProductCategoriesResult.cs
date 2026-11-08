using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts.Categories;

/// <summary>The result of the write of a product's categories (see <see cref="ProductCategoriesOutcome"/>); a refusal is a value and rolls the transaction back.</summary>
public sealed class ProductCategoriesResult : IOperationResult
{
    private ProductCategoriesResult(ProductCategoriesOutcome outcome)
    {
        Outcome = outcome;
    }

    public static ProductCategoriesResult Written { get; } = new(ProductCategoriesOutcome.Written);

    public ProductCategoriesOutcome Outcome { get; }

    public bool IsRejected => Outcome != ProductCategoriesOutcome.Written;

    public static ProductCategoriesResult Refused(ProductCategoriesOutcome outcome)
    {
        if (outcome == ProductCategoriesOutcome.Written)
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "A written result is the shared value.");
        }

        return new ProductCategoriesResult(outcome);
    }
}
