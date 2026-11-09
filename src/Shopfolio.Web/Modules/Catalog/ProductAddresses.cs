using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The address of a product (T363; 02 3.30.1, 3.30.2; 06 4.72): made from the product's name on the server, never from the category it hangs on - a product moved to another
/// category keeps its address - and never typed by the firm. Until the first publication the address follows the name, from the first publication it is fixed
/// (<see cref="Product.SlugFixedAt"/>). A draft's and an archived product's address counts as used; a permanently deleted product's row is gone, so its address is free
/// again (02 3.30.2: K-85, K-568). The two functions are <see cref="Slug.ForSaveAsync"/> and <see cref="Product.SetAddress"/>; the product form writes through this class.
/// </summary>
internal static class ProductAddresses
{
    /// <summary>
    /// The address <paramref name="name"/> gives a product that is saved with <paramref name="currentSlug"/> and <paramref name="fixedAt"/> (empty for a new product and for one that
    /// was never published): the name's address or the first numbered one no other product holds. A fixed address is returned as it is.
    /// </summary>
    public static Task<string> ForSaveAsync(AppDbContext db, string name, string currentSlug, DateTimeOffset? fixedAt, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        return Slug.ForSaveAsync(
            name,
            AddressRules.ProductPrefix,
            currentSlug,
            fixedAt,
            async (baseSlug, cancel) => await db.Products.AsNoTracking().Where(p => p.Slug.StartsWith(baseSlug)).Select(p => p.Slug).ToListAsync(cancel),
            ct);
    }
}
