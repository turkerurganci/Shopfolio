using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one place that says which variants of a digital product have a file (02 3.7.4; 06 4.70): a variant is bound to a file when it carries a current file of its own or its
/// product carries one - a current file is a row linked to the owner and not detached, the same reading as <see cref="DigitalFileReader"/>. The publish gate asks it for the
/// move to Published and the file removal asks it for the move it would cause, so the two cannot disagree. <c>exceptFileId</c> reads the picture as if that file were already
/// removed. A plain read; it writes nothing.
/// </summary>
internal static class DigitalFileCoverage
{
    /// <summary>The Published variants of the product that no current file serves - the ones a digital product's gate counts and the ones the panel shows as missing a file.</summary>
    public static async Task<IReadOnlyList<Guid>> FindPublishedWithoutFileAsync(AppDbContext db, Guid productId, Guid? exceptFileId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        List<Guid> published = await db.Variants.AsNoTracking()
            .Where(v => v.ProductId == productId && v.PublishStatus == PublishStatus.Published)
            .OrderBy(v => v.CreatedAt).ThenBy(v => v.Id)
            .Select(v => v.Id)
            .ToListAsync(ct);
        if (published.Count == 0 || await HasProductFileAsync(db, productId, exceptFileId, ct))
        {
            return [];
        }

        Guid except = exceptFileId ?? Guid.Empty;
        HashSet<Guid> served = [.. await db.DigitalFiles.AsNoTracking()
            .Where(f => f.DetachedAt == null && f.Id != except && f.VariantId != null && published.Contains(f.VariantId.Value))
            .Select(f => f.VariantId!.Value)
            .ToListAsync(ct)];
        return [.. published.Where(id => !served.Contains(id))];
    }

    /// <summary>Whether one variant is bound to a file: its own current file or its product's.</summary>
    public static async Task<bool> HasFileAsync(AppDbContext db, Guid variantId, Guid productId, Guid? exceptFileId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        Guid except = exceptFileId ?? Guid.Empty;
        return await db.DigitalFiles.AsNoTracking().AnyAsync(f => f.DetachedAt == null && f.Id != except && f.VariantId == variantId, ct)
            || await HasProductFileAsync(db, productId, exceptFileId, ct);
    }

    private static Task<bool> HasProductFileAsync(AppDbContext db, Guid productId, Guid? exceptFileId, CancellationToken ct)
    {
        Guid except = exceptFileId ?? Guid.Empty;
        return db.DigitalFiles.AsNoTracking().AnyAsync(f => f.DetachedAt == null && f.Id != except && f.ProductId == productId, ct);
    }
}
