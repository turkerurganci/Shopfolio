using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>One cart line as the quantity rules read it: the quantity and the facts of its variant and product. <paramref name="Stock"/> is empty when the count is not tracked.</summary>
internal sealed record QuantityInput(Guid VariantId, Guid ProductId, int Quantity, int? Stock, int Reserved, int? OrderLimit, bool IsOnShowcase);

/// <summary>
/// The cart's quantity rules as pure functions (T347; 02 3.16.8-3.16.10; 10 KD-7, KD-35): the quantity may not exceed the stock less the reserved units - the service capacity follows the
/// same rule - and the quantities of all variants of a product together may not exceed the firm's "bir siparişte en fazla" limit. A line over the stock does not count into the total, and
/// a line that does not enter the order for another reason (off the showcase, sold out) does not either (02 3.16.10). Nothing here reads the database; the reader applies it.
/// </summary>
internal static class CartQuantityRules
{
    /// <summary>Whether the quantity is above what can be bought now: the count is tracked and the quantity is more than the stock less the reserved units (02 3.16.8).</summary>
    public static bool ExceedsStock(int? stock, int reserved, int quantity) => stock is not null && quantity > stock - reserved;

    /// <summary>Whether the line enters the order before the per-order limit is looked at: on the showcase, not sold out, and a quantity the stock holds.</summary>
    public static bool EntersBeforeLimit(QuantityInput line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line.IsOnShowcase && !StockRules.IsSoldOut(line.Stock, line.Reserved) && !ExceedsStock(line.Stock, line.Reserved, line.Quantity);
    }

    /// <summary>
    /// The verdict of each line of a cart that is read (02 3.16.8, 3.16.10). A sold-out or off-showcase line has its own mark and no quantity problem. A line over the stock is
    /// <see cref="CartQuantityProblem.NoStock"/> and stays out of the totals. The lines that remain are summed per product: above the limit, every one of them is
    /// <see cref="CartQuantityProblem.OverOrderLimit"/> until the total is back within it.
    /// </summary>
    public static IReadOnlyList<CartQuantityVerdict> Judge(IReadOnlyList<QuantityInput> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        Dictionary<Guid, long> totals = lines
            .Where(EntersBeforeLimit)
            .GroupBy(l => l.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(l => (long)l.Quantity));

        List<CartQuantityVerdict> verdicts = [];
        foreach (QuantityInput line in lines)
        {
            if (!line.IsOnShowcase || StockRules.IsSoldOut(line.Stock, line.Reserved))
            {
                verdicts.Add(new CartQuantityVerdict(line.VariantId, CartQuantityProblem.None, null));
            }
            else if (ExceedsStock(line.Stock, line.Reserved, line.Quantity))
            {
                verdicts.Add(new CartQuantityVerdict(line.VariantId, CartQuantityProblem.NoStock, null));
            }
            else if (line.OrderLimit is int limit && totals[line.ProductId] > limit)
            {
                verdicts.Add(new CartQuantityVerdict(line.VariantId, CartQuantityProblem.OverOrderLimit, limit));
            }
            else
            {
                verdicts.Add(new CartQuantityVerdict(line.VariantId, CartQuantityProblem.None, null));
            }
        }

        return verdicts;
    }

    /// <summary>
    /// The verdict on an add (02 3.16.8, 3.16.9): <paramref name="candidate"/> carries the quantity the line would hold after the add, <paramref name="others"/> are the cart's other lines.
    /// The firm's limit is judged first, over the candidate and the other lines of the product that enter the order; when the quantity breaks both the limit and the stock only the limit is
    /// told. Then the stock. The candidate counts into the total even when it is over the stock - that is what lets the limit win.
    /// </summary>
    public static CartQuantityVerdict JudgeAdd(QuantityInput candidate, IReadOnlyList<QuantityInput> others)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(others);
        if (candidate.OrderLimit is int limit)
        {
            long total = (long)candidate.Quantity + others.Where(o => o.ProductId == candidate.ProductId && o.VariantId != candidate.VariantId && EntersBeforeLimit(o)).Sum(o => (long)o.Quantity);
            if (total > limit)
            {
                return new CartQuantityVerdict(candidate.VariantId, CartQuantityProblem.OverOrderLimit, limit);
            }
        }

        return new CartQuantityVerdict(
            candidate.VariantId,
            ExceedsStock(candidate.Stock, candidate.Reserved, candidate.Quantity) ? CartQuantityProblem.NoStock : CartQuantityProblem.None,
            null);
    }
}
