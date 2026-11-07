using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Variants;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The plain reads of a product's structure (02 3.3.2, 3.3.3; 06 4.6-4.8): the verdict on the option values a variant would be bound to, and the product's option
/// matrix. Nothing is written and no lock is taken: the product form's own version is what keeps two administrators from saving over each other (09 10.2), and the
/// unique indexes of the variant are the last defense of a combination or a stock code two saves race for.
/// </summary>
internal sealed class ProductStructureReader
{
    private readonly AppDbContext _db;

    public ProductStructureReader(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// The verdict on the values a variant of <paramref name="productId"/> is bound to; <paramref name="exceptVariantId"/> is the variant being edited, which does not
    /// count against its own combination. The bound values are checked first (the order of the verdicts of <see cref="OptionStructureRules.CheckBinding"/>), the
    /// combination last. A product without options takes the one variant with both values empty.
    /// </summary>
    public async Task<VariantStructureCheck> CheckVariantAsync(
        Guid productId,
        Guid? firstOptionValueId,
        Guid? secondOptionValueId,
        Guid? exceptVariantId,
        CancellationToken ct)
    {
        List<(Guid Id, int SortOrder)> options = (await _db.Options.AsNoTracking()
            .Where(o => o.ProductId == productId)
            .Select(o => new { o.Id, o.SortOrder })
            .ToListAsync(ct)).ConvertAll(o => (o.Id, o.SortOrder));
        HashSet<int> orders = [.. options.Select(o => o.SortOrder)];

        BoundValue first = await BindAsync(firstOptionValueId, options, ct);
        BoundValue second = await BindAsync(secondOptionValueId, options, ct);
        VariantStructureCheck binding = OptionStructureRules.CheckBinding(orders, first, second);
        if (binding != VariantStructureCheck.Valid)
        {
            return binding;
        }

        bool taken = await _db.Variants.AsNoTracking().AnyAsync(
            v => v.ProductId == productId
                && v.FirstOptionValueId == firstOptionValueId
                && v.SecondOptionValueId == secondOptionValueId
                && v.Id != exceptVariantId,
            ct);
        return taken ? VariantStructureCheck.CombinationTaken : VariantStructureCheck.Valid;
    }

    /// <summary>The matrix of the product's options, each cell with the variant that opens it or none (02 3.3.3); every variant of the product counts, whatever its status.</summary>
    public async Task<IReadOnlyList<OptionMatrixCell>> GetMatrixAsync(Guid productId, CancellationToken ct)
    {
        var options = await _db.Options.AsNoTracking()
            .Where(o => o.ProductId == productId)
            .Select(o => new { o.Id, o.SortOrder })
            .ToListAsync(ct);
        Guid? firstOption = options.Where(o => o.SortOrder == 1).Select(o => (Guid?)o.Id).SingleOrDefault();
        Guid? secondOption = options.Where(o => o.SortOrder == 2).Select(o => (Guid?)o.Id).SingleOrDefault();

        List<Guid> firstValues = await ValuesOfAsync(firstOption, ct);
        List<Guid> secondValues = await ValuesOfAsync(secondOption, ct);
        var variants = await _db.Variants.AsNoTracking()
            .Where(v => v.ProductId == productId)
            .Select(v => new { v.Id, v.FirstOptionValueId, v.SecondOptionValueId })
            .ToListAsync(ct);
        return OptionStructureRules.BuildMatrix(
            firstValues,
            secondValues,
            [.. variants.Select(v => (v.Id, v.FirstOptionValueId, v.SecondOptionValueId))]);
    }

    private async Task<BoundValue> BindAsync(Guid? valueId, IReadOnlyList<(Guid Id, int SortOrder)> productOptions, CancellationToken ct)
    {
        if (valueId is null)
        {
            return BoundValue.None;
        }

        Guid? optionId = await _db.OptionValues.AsNoTracking()
            .Where(v => v.Id == valueId)
            .Select(v => (Guid?)v.OptionId)
            .SingleOrDefaultAsync(ct);
        foreach ((Guid id, int sortOrder) in productOptions)
        {
            if (id == optionId)
            {
                return BoundValue.Of(sortOrder);
            }
        }

        return BoundValue.Of(null);
    }

    private async Task<List<Guid>> ValuesOfAsync(Guid? optionId, CancellationToken ct)
    {
        if (optionId is null)
        {
            return [];
        }

        return await _db.OptionValues.AsNoTracking()
            .Where(v => v.OptionId == optionId)
            .OrderBy(v => v.SortOrder)
            .ThenBy(v => v.CreatedAt)
            .ThenBy(v => v.Id)
            .Select(v => v.Id)
            .ToListAsync(ct);
    }
}
