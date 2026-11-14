using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The planes' files of one product and the items' file links they move (06 4.70 "Uygulamada", 4.18; K-1029 (4), K-1119; T525, D-527). A variant's current file is its own attached,
/// not detached file, else its product's - the reading of <see cref="DigitalFileCoverage"/> and <see cref="DigitalFileReader"/>. The writer reads the planes before its tracked change,
/// states the planes after it, and <see cref="Changes"/> gives each variant whose current file moved the file its items download after the write: the new current file, or the last one
/// when none is left (the item keeps the file it last had). The items themselves are the Ordering module's (<c>IOrderingModule.AdvanceItemFilesAsync</c>; 09 6).
/// </summary>
internal static class OrderItemFiles
{
    /// <summary>The current files of the product's planes as the database holds them now (the caller's unsaved changes are not seen).</summary>
    public static async Task<FilePlanes> ReadAsync(AppDbContext db, Guid productId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        List<Guid> variants = await db.Variants.AsNoTracking().Where(v => v.ProductId == productId).Select(v => v.Id).ToListAsync(ct);
        var files = await db.DigitalFiles.AsNoTracking()
            .Where(f => f.DetachedAt == null && f.AttachedAt != null
                && (f.ProductId == productId || (f.VariantId != null && variants.Contains(f.VariantId.Value))))
            .Select(f => new { f.Id, f.ProductId, f.VariantId })
            .ToListAsync(ct);
        Guid? productFile = files.Where(f => f.ProductId == productId).Select(f => (Guid?)f.Id).SingleOrDefault();
        Dictionary<Guid, Guid> variantFiles = files
            .Where(f => f.VariantId is not null)
            .ToDictionary(f => f.VariantId.GetValueOrDefault(), f => f.Id);
        return new FilePlanes(variants, productFile, variantFiles);
    }

    /// <summary>The file each listed variant's items download after the write, for the variants whose current file moved; a variant that had no file and has none is not listed.</summary>
    public static Dictionary<Guid, Guid> Changes(IEnumerable<Guid> variantIds, FilePlanes before, FilePlanes after)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        Dictionary<Guid, Guid> changes = [];
        foreach (Guid variantId in variantIds)
        {
            Guid? was = before.CurrentOf(variantId);
            Guid? now = after.CurrentOf(variantId);
            if (was != now && (now ?? was) is Guid file)
            {
                changes[variantId] = file;
            }
        }

        return changes;
    }

    /// <summary>The last file of each variant that goes with a deletion: its current file, which its items keep as their last known file.</summary>
    public static Dictionary<Guid, Guid> LastFiles(IEnumerable<Guid> variantIds, FilePlanes planes)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        ArgumentNullException.ThrowIfNull(planes);
        Dictionary<Guid, Guid> files = [];
        foreach (Guid variantId in variantIds)
        {
            if (planes.CurrentOf(variantId) is Guid file)
            {
                files[variantId] = file;
            }
        }

        return files;
    }

    /// <summary>The current file of a product's plane and of each variant's own plane.</summary>
    public sealed record FilePlanes(IReadOnlyList<Guid> Variants, Guid? ProductFile, IReadOnlyDictionary<Guid, Guid> VariantFiles)
    {
        public Guid? CurrentOf(Guid variantId) => VariantFiles.TryGetValue(variantId, out Guid file) ? file : ProductFile;

        /// <summary>The planes once one file is gone from them (a removal).</summary>
        public FilePlanes Without(Guid fileId)
            => new(Variants, ProductFile == fileId ? null : ProductFile, VariantFiles.Where(p => p.Value != fileId).ToDictionary(p => p.Key, p => p.Value));
    }
}
