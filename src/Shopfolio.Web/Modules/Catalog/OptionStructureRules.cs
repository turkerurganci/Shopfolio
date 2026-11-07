using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Variants;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The option value of one slot of a variant as the rule reads it: whether the variant names a value there, and - when it does - the order (1 or 2) of the option of
/// the product that value belongs to; <c>null</c> when the value belongs to no option of this product (another product's value, an unknown id).
/// </summary>
internal readonly record struct BoundValue(bool IsGiven, int? OwnerOrder)
{
    public static BoundValue None { get; } = new(false, null);

    public static BoundValue Of(int? ownerOrder) => new(true, ownerOrder);
}

/// <summary>
/// The structure rules of option dimensions and variant bindings as pure functions (02 3.3.2, 3.3.3; 06 4.6, 4.7 "Uygulamada"; 11 T332 criteria 3-5). A product carries
/// at most <see cref="ProductConstants.OptionDimensionMax"/> option dimensions (P-21 - the count is read from the constant, never written here); a variant is bound to
/// the first value of the option of order 1 and the second value of the option of order 2, each required when the product has that option and forbidden when it has
/// not; the matrix of two dimensions need not be full - a combination no variant opens is shown and cannot be chosen.
/// </summary>
internal static class OptionStructureRules
{
    /// <summary>
    /// The order the next option of a product takes: the lowest free order from 1 up to the dimension limit; <c>null</c> when the product already has the most
    /// dimensions it may (the third dimension is refused).
    /// </summary>
    public static int? NextOptionOrder(IEnumerable<int> existingOrders)
    {
        ArgumentNullException.ThrowIfNull(existingOrders);
        HashSet<int> taken = [.. existingOrders];
        for (int order = 1; order <= ProductConstants.OptionDimensionMax; order++)
        {
            if (!taken.Contains(order))
            {
                return order;
            }
        }

        return null;
    }

    /// <summary>
    /// Whether the two bound values fit a product whose options have the given orders. The first slot belongs to the option of order 1, the second to the option of
    /// order 2; a slot is required when its option exists and must be empty when it does not.
    /// </summary>
    public static VariantStructureCheck CheckBinding(IReadOnlySet<int> optionOrders, BoundValue first, BoundValue second)
    {
        ArgumentNullException.ThrowIfNull(optionOrders);
        VariantStructureCheck firstVerdict = CheckSlot(optionOrders.Contains(1), 1, first);
        return firstVerdict != VariantStructureCheck.Valid ? firstVerdict : CheckSlot(optionOrders.Contains(2), 2, second);
    }

    /// <summary>
    /// The matrix of a product's options: the cells of the cartesian product of the option values - a cell per first value when there is one dimension, one cell with
    /// both values empty when there is none - each carrying the variant that opens it, or none. The values come in the order the storefront lists them. A variant that
    /// binds a pair that is not among the values (a stale binding) opens no cell.
    /// </summary>
    public static IReadOnlyList<OptionMatrixCell> BuildMatrix(
        IReadOnlyList<Guid> firstValues,
        IReadOnlyList<Guid> secondValues,
        IReadOnlyCollection<(Guid VariantId, Guid? First, Guid? Second)> variants)
    {
        ArgumentNullException.ThrowIfNull(firstValues);
        ArgumentNullException.ThrowIfNull(secondValues);
        ArgumentNullException.ThrowIfNull(variants);
        Dictionary<(Guid?, Guid?), Guid> opened = [];
        foreach ((Guid variantId, Guid? first, Guid? second) in variants)
        {
            opened.TryAdd((first, second), variantId);
        }

        IReadOnlyList<Guid?> firsts = firstValues.Count == 0 ? [null] : [.. firstValues.Select(v => (Guid?)v)];
        IReadOnlyList<Guid?> seconds = secondValues.Count == 0 ? [null] : [.. secondValues.Select(v => (Guid?)v)];
        List<OptionMatrixCell> cells = [];
        foreach (Guid? first in firsts)
        {
            foreach (Guid? second in seconds)
            {
                cells.Add(new OptionMatrixCell(first, second, opened.TryGetValue((first, second), out Guid variantId) ? variantId : null));
            }
        }

        return cells;
    }

    private static VariantStructureCheck CheckSlot(bool optionExists, int slotOrder, BoundValue value)
    {
        if (!value.IsGiven)
        {
            return optionExists ? VariantStructureCheck.ValueMissing : VariantStructureCheck.Valid;
        }

        if (!optionExists)
        {
            return VariantStructureCheck.ValueForAbsentOption;
        }

        return value.OwnerOrder == slotOrder ? VariantStructureCheck.Valid : VariantStructureCheck.ValueNotOfTheProduct;
    }
}
