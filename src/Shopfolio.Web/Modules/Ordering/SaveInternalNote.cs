using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The internal note's two conditional statements (09 10.9 item 3; 06 4.20; K-1051): the note is written or deleted only while its text is still the text
/// the administrator's form showed. They run inside the caller's transaction, after the order's shared row lock (<see cref="OrderLocks.LockSharedAsync"/>);
/// <see cref="OrderCorrections.SaveInternalNoteAsync"/> is the unit that orders the steps. The note is outside the order's version, so they touch only the
/// note's own table - never the order's row (09 10.9 item 3) - and this file is the one place of a bulk write on the note (a source scan holds it to these two).
/// </summary>
internal static class SaveInternalNote
{
    /// <summary>Writes the new text and its writer when the note's text is still <paramref name="shownText"/>; the number of rows written (0 or 1).</summary>
    public static Task<int> UpdateShownAsync(AppDbContext db, Guid orderId, string shownText, string text, Guid adminId, string adminName, CancellationToken ct)
    {
        return db.InternalNotes
            .Where(n => n.OrderId == orderId && n.Text == shownText)
            .ExecuteUpdateAsync(
                s => s.SetProperty(n => n.Text, text)
                    .SetProperty(n => n.AdminId, adminId)
                    .SetProperty(n => n.AdminName, adminName),
                ct);
    }

    /// <summary>Deletes the note when its text is still <paramref name="shownText"/> (an emptied note is not stored; K-1051); the number of rows deleted (0 or 1).</summary>
    public static Task<int> DeleteShownAsync(AppDbContext db, Guid orderId, string shownText, CancellationToken ct)
    {
        return db.InternalNotes
            .Where(n => n.OrderId == orderId && n.Text == shownText)
            .ExecuteDeleteAsync(ct);
    }
}
