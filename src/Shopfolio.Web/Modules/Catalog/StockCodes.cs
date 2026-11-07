using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts.Variants;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The reads behind the variant form's stock code field (02 3.3.4; 05 7.8.1, MK-32; 06 2.8.2): whether a typed code is free, and the code the form suggests for a new
/// variant. The code is compared in its identity form (<see cref="TextNormalizer.NormalizeIdentity"/>), the column of the unique index, and is kept and shown as typed;
/// the suggestion is built by <see cref="StockCodeSuggestion"/> from the product's address, so the folding is the one common function. Nothing is written.
/// </summary>
internal sealed class StockCodes
{
    private readonly AppDbContext _db;

    public StockCodes(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Whether <paramref name="code"/> is usable: missing when it is null or blank, taken when another variant holds it in the identity form
    /// (<paramref name="exceptVariantId"/> is the variant being edited, which does not collide with its own code), free otherwise.
    /// </summary>
    public async Task<StockCodeCheck> CheckAsync(string? code, Guid? exceptVariantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return StockCodeCheck.Missing;
        }

        string normalized = TextNormalizer.NormalizeIdentity(code);
        bool taken = await _db.Variants.AsNoTracking().AnyAsync(v => v.NormalizedSKU == normalized && v.Id != exceptVariantId, ct);
        return taken ? StockCodeCheck.Taken : StockCodeCheck.Free;
    }

    /// <summary>
    /// The code suggested for a new variant (05 7.8.1). A saved product's suggestion starts from its address (its name may have changed since; the codes already given
    /// do not), the order is the number of its variants plus one; a product not saved yet starts from the name as it is now, at order 1; a blank name with no saved
    /// product gives no suggestion (<c>null</c>) and the field stays required. A code another variant holds raises the order until it is free.
    /// </summary>
    public async Task<string?> SuggestAsync(Guid? productId, string? productName, CancellationToken ct)
    {
        string? address = null;
        int order = 1;
        if (productId is Guid id)
        {
            address = await _db.Products.AsNoTracking().Where(p => p.Id == id).Select(p => p.Slug).SingleOrDefaultAsync(ct);
            if (address is not null)
            {
                order = await _db.Variants.AsNoTracking().CountAsync(v => v.ProductId == id, ct) + 1;
            }
        }

        if (address is null)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                return null;
            }

            address = Slug.From(productName, "urun");
        }

        // The codes that can collide are the ones that start with the same head: the address holds only [a-z0-9-], so no LIKE meta character can be in the prefix.
        string prefix = TextNormalizer.NormalizeIdentity(StockCodeSuggestion.Prefix(address) + "-");
        List<string> used = await _db.Variants.AsNoTracking()
            .Where(v => v.NormalizedSKU.StartsWith(prefix))
            .Select(v => v.SKU)
            .ToListAsync(ct);
        return StockCodeSuggestion.Suggest(address, order, used);
    }
}
