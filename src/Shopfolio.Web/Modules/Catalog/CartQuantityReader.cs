using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The plain read behind the cart's quantity verdicts (T347; 02 3.16.8-3.16.10): per variant the stock, the reserved units, the product and the product's "bir siparişte en fazla" limit,
/// judged by <see cref="CartQuantityRules"/>. The stock is read with the reserved units taken off. Nothing is written and no lock is taken: the verdict is the cart's, the reservation at the order
/// is the conditional write of the order (05 7.3.3).
/// </summary>
internal sealed class CartQuantityReader
{
    private readonly AppDbContext _db;

    public CartQuantityReader(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>The verdict on each line of a cart that is read; a line whose variant does not exist is not in the answer.</summary>
    public async Task<IReadOnlyList<CartQuantityVerdict>> JudgeAsync(IReadOnlyList<CartQuantityLine> lines, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(lines);
        IReadOnlyList<QuantityInput> inputs = await LoadAsync(lines, ct);
        return CartQuantityRules.Judge(inputs);
    }

    /// <summary>
    /// The verdict on an add (02 3.16.8, 3.16.9): <paramref name="candidate"/> is the variant with the quantity the line would hold after the add, <paramref name="others"/> the cart's other
    /// lines. A variant that does not exist gets <see cref="CartQuantityProblem.None"/>: the caller has already answered that it is not there.
    /// </summary>
    public async Task<CartQuantityVerdict> JudgeAddAsync(CartQuantityLine candidate, IReadOnlyList<CartQuantityLine> others, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(others);
        IReadOnlyList<QuantityInput> inputs = await LoadAsync([candidate, .. others.Where(o => o.VariantId != candidate.VariantId)], ct);
        QuantityInput? found = inputs.FirstOrDefault(i => i.VariantId == candidate.VariantId);
        if (found is null)
        {
            return new CartQuantityVerdict(candidate.VariantId, CartQuantityProblem.None, null);
        }

        return CartQuantityRules.JudgeAdd(found, [.. inputs.Where(i => i.VariantId != candidate.VariantId)]);
    }

    private async Task<IReadOnlyList<QuantityInput>> LoadAsync(IReadOnlyList<CartQuantityLine> lines, CancellationToken ct)
    {
        Dictionary<Guid, int> quantities = lines.GroupBy(l => l.VariantId).ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));
        if (quantities.Count == 0)
        {
            return [];
        }

        Guid[] ids = [.. quantities.Keys];
        var rows = await _db.Variants.AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .Join(
                _db.Products.AsNoTracking(),
                v => v.ProductId,
                p => p.Id,
                (v, p) => new
                {
                    VariantId = v.Id,
                    v.ProductId,
                    ProductStatus = p.PublishStatus,
                    VariantStatus = v.PublishStatus,
                    v.StockQuantity,
                    v.ReservedQuantity,
                    p.MaxQuantityPerOrder,
                })
            .ToListAsync(ct);

        return
        [
            .. rows.Select(r => new QuantityInput(
                r.VariantId,
                r.ProductId,
                quantities[r.VariantId],
                r.StockQuantity,
                r.ReservedQuantity,
                r.MaxQuantityPerOrder,
                r.ProductStatus == PublishStatus.Published && r.VariantStatus == PublishStatus.Published)),
        ];
    }
}
