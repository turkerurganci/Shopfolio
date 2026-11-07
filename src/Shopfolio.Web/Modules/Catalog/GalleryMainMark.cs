using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one bulk statement of the gallery (T339; 06 4.71; 09 10.2 items 2-4): it takes the main mark off a product's current main image before the new one is written. The database allows one
/// main image per product at every moment (a unique index, not a deferred check), and a save sends the changed rows in no order the code controls, so a main mark moved by two tracked updates
/// can meet the index half-way. The statement runs inside the caller's open transaction and is the only bulk write of the gallery; it never passes the product version guard, which is safe
/// because it is sent only when the same save changes another link (the new main mark, or the removal of the old main link), and that change raises the product's version.
/// It lives alone in this file so that the closed list of places of the bulk write scan names this statement and nothing else of the gallery.
/// </summary>
internal static class GalleryMainMark
{
    public static async Task ClearAsync(AppDbContext db, Guid productId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The main mark is cleared only inside the caller's transaction (09 9.1 item 3).");
        }

        await db.ProductImages.Where(p => p.ProductId == productId && p.IsMain).ExecuteUpdateAsync(s => s.SetProperty(p => p.IsMain, false), ct);
    }
}
