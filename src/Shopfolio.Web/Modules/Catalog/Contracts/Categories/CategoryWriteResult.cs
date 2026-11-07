using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts.Categories;

/// <summary>
/// The result of a category write (create, rename, move, delete): the outcome, the category it concerns and - when the delete is blocked - the numbers that
/// block it, which the panel shows (02 3.4.5). A refusal is a value, not an exception; the transaction runner rolls the work back on it (09 9.1 item 3).
/// </summary>
public sealed class CategoryWriteResult : IOperationResult
{
    private CategoryWriteResult(CategoryWriteOutcome outcome, Guid? categoryId, int blockingChildren, int blockingProducts)
    {
        Outcome = outcome;
        CategoryId = categoryId;
        BlockingChildren = blockingChildren;
        BlockingProducts = blockingProducts;
    }

    public CategoryWriteOutcome Outcome { get; }

    /// <summary>The category written; empty when the category does not exist.</summary>
    public Guid? CategoryId { get; }

    /// <summary>The sub-categories that block the delete; 0 for every other outcome.</summary>
    public int BlockingChildren { get; }

    /// <summary>The products hung directly on the category that block the delete; 0 for every other outcome.</summary>
    public int BlockingProducts { get; }

    public bool IsRejected => Outcome != CategoryWriteOutcome.Written;

    public static CategoryWriteResult Written(Guid categoryId) => new(CategoryWriteOutcome.Written, categoryId, 0, 0);

    public static CategoryWriteResult Refused(CategoryWriteOutcome outcome, Guid? categoryId = null)
    {
        if (outcome is CategoryWriteOutcome.Written or CategoryWriteOutcome.NotEmpty)
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "A written result and a blocked delete have their own factories.");
        }

        return new CategoryWriteResult(outcome, categoryId, 0, 0);
    }

    public static CategoryWriteResult NotEmpty(Guid categoryId, int children, int products)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(children);
        ArgumentOutOfRangeException.ThrowIfNegative(products);
        return new CategoryWriteResult(CategoryWriteOutcome.NotEmpty, categoryId, children, products);
    }
}
